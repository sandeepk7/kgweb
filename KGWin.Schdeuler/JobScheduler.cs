using Quartz;
using Microsoft.Data.Sqlite;

namespace KGWin.Schdeuler
{
    public class JobScheduler : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // Read connection string from JobDataMap
            string connectionString = context.MergedJobDataMap.GetString("ConnectionString")!;

            try
            {
                await using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                // Read current schedule
                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = @"
                    SELECT Id, ScheduleDateTime
                    FROM SchedulerWorker
                    ORDER BY datetime(ScheduleDateTime) ASC
                    LIMIT 1;
                ";

                await using var reader = await selectCommand.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int id = reader.GetInt32(0);
                    DateTime currentSchedule = reader.GetDateTime(1);
                    Console.WriteLine($"[{DateTime.Now}] Job executed at {currentSchedule}");

                    // Add 5 minutes to schedule
                    DateTime newSchedule = currentSchedule.AddMinutes(5);

                    // Update DB
                    var updateCommand = connection.CreateCommand();
                    updateCommand.CommandText = @"
                        UPDATE SchedulerWorker
                        SET ScheduleDateTime = @newTime
                        WHERE Id = @id;
                    ";
                    updateCommand.Parameters.AddWithValue("@newTime", newSchedule);
                    updateCommand.Parameters.AddWithValue("@id", id);

                    await updateCommand.ExecuteNonQueryAsync();
                    Console.WriteLine($"Schedule updated to {newSchedule}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HelloJob: {ex.Message}");
            }
        }
    }
}