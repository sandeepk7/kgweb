using Microsoft.Data.Sqlite;
using Quartz;

namespace KGWin.Schdeuler
{
    public class Worker : BackgroundService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<Worker> _logger;
        //private readonly string _connectionString = @"Data Source=C:\Users\user\Downloads\QuartzDemo\Database\Worker.db";

        private readonly string _connectionString;

        public Worker(ISchedulerFactory schedulerFactory, ILogger<Worker> logger, IConfiguration configuration)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("Default")!;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);
            await scheduler.Start(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var nextSchedule = await GetNextScheduleTimeAsync();

                if (nextSchedule != null && nextSchedule > DateTime.Now)
                {
                    var jobKey = new JobKey(typeof(JobScheduler).Name, "default");
                    var triggerKey = new TriggerKey("helloTrigger", "default");

                    // Create job if it doesn't exist (durable)
                    if (!await scheduler.CheckExists(jobKey, stoppingToken))
                    {
                        var job = JobBuilder.Create<JobScheduler>()
                                            .WithIdentity(jobKey)
                                            .StoreDurably() // Job must be durable
                                            .UsingJobData("ConnectionString", _connectionString)
                                            .Build();

                        await scheduler.AddJob(job, true, stoppingToken);
                        _logger.LogInformation("Job created as durable.");
                    }

                    // Always create a trigger for the next schedule
                    var trigger = TriggerBuilder.Create()
                                                .WithIdentity(triggerKey)
                                                .ForJob(jobKey)
                                                .StartAt(nextSchedule.Value)
                                                .Build();

                    // Schedule or reschedule the trigger
                    if (await scheduler.CheckExists(triggerKey, stoppingToken))
                    {
                        await scheduler.RescheduleJob(triggerKey, trigger, stoppingToken);
                        _logger.LogInformation($"Rescheduled job to {nextSchedule}");
                    }
                    else
                    {
                        await scheduler.ScheduleJob(trigger, stoppingToken);
                        _logger.LogInformation($"Scheduled job at {nextSchedule}");
                    }
                }

                // Check DB every 30 seconds for new schedule
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task<DateTime?> GetNextScheduleTimeAsync()
        {
            try
            {
                await using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT ScheduleDateTime
                    FROM SchedulerWorker
                    ORDER BY datetime(ScheduleDateTime) ASC
                    LIMIT 1;
                ";

                var result = await command.ExecuteScalarAsync();
                if (result != null && DateTime.TryParse(result.ToString(), out var dt))
                    return dt;
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error querying Scheduler table: {ex.Message}");
            }

            return null;
        }
    }
}

