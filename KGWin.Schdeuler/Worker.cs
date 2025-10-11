using Quartz;
using System.Text.Json;

namespace KGWin.Schdeuler
{
    public class Worker : BackgroundService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private const string LastRunFileName = "job_schedule_history.json";
        private readonly string _filePath;

        public Worker(ISchedulerFactory schedulerFactory, ILogger<Worker> logger, IConfiguration configuration)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
            _configuration = configuration;            
            string rootPath = Directory.GetCurrentDirectory();
            _filePath = Path.Combine(rootPath, LastRunFileName);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Quartz Worker...");

            var lastRunEntries = await LoadLastRunEntriesAsync();

            var cronJobs = _configuration.GetSection("JobConfig:CronJobs")
                                         .Get<List<CronJobConfig>>() ?? new();

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);
            await scheduler.Start(stoppingToken);

            foreach (var cronJob in cronJobs)
            {
                var jobKey = new JobKey(cronJob.JobName, "CronGroup");

                var job = JobBuilder.Create<JobScheduler>()
                    .WithIdentity(jobKey)
                    .UsingJobData("JobName", cronJob.JobName)
                    .UsingJobData("LastRunFilePath", _filePath)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{cronJob.JobName}_Trigger", "CronGroup")
                    .WithCronSchedule(cronJob.CronSchedule)
                    .ForJob(job)
                    .Build();

                await scheduler.ScheduleJob(job, trigger, stoppingToken);

                _logger.LogInformation($"Scheduled '{cronJob.JobName}' with cron '{cronJob.CronSchedule}'");

                // Check for missed execution
                if (WasJobMissed(cronJob, lastRunEntries))
                {
                    _logger.LogWarning($" Missed job detected: {cronJob.JobName}. Running it now...");
                    await RunMissedJobManually(cronJob.JobName);
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Quartz Worker stopping...");
        }

        private async Task RunMissedJobManually(string jobName)
        {
            var job = new JobScheduler();
            await job.RunManually(jobName, _filePath);
        }

        private async Task<List<LastRunEntry>> LoadLastRunEntriesAsync()
        {
            if (!File.Exists(_filePath))
                return new();

            var json = await File.ReadAllTextAsync(_filePath);

            if (string.IsNullOrWhiteSpace(json))
                return new();

            return JsonSerializer.Deserialize<List<LastRunEntry>>(json) ?? new();
        }

        private bool WasJobMissed(CronJobConfig jobConfig, List<LastRunEntry> history)
        {
            var entry = history.FirstOrDefault(x => x.JobName == jobConfig.JobName);
            if (entry == null)
                return true; // Never run before — must be run once

            var cronExpr = new CronExpression(jobConfig.CronSchedule);
            var lastRun = entry.LastRun;

            // Check for any fire times between the last run and now
            var fireTimes = GetFireTimesBetween(cronExpr, lastRun, DateTimeOffset.UtcNow);

            return fireTimes.Count > 0 ? true : false; 
        }


        private List<DateTimeOffset> GetFireTimesBetween(CronExpression cron, DateTimeOffset from, DateTimeOffset to)
        {
            var times = new List<DateTimeOffset>();
            var next = cron.GetNextValidTimeAfter(from);

            if(next.HasValue && next.Value > to)
            {
                times.Add(next.Value);
                next = cron.GetNextValidTimeAfter(next.Value);
            }

            return times;
        }

        public class CronJobConfig
        {
            public string JobName { get; set; } = default!;
            public string CronSchedule { get; set; } = default!;
        }

        public class LastRunEntry
        {
            public string JobName { get; set; } = default!;
            public DateTimeOffset LastRun { get; set; }
        }
    }
}
