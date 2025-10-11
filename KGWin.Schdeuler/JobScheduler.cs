using Quartz;
using System.Text.Json;
using static KGWin.Schdeuler.Worker;

namespace KGWin.Schdeuler
{
    public class JobScheduler : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            string jobName = context.JobDetail.JobDataMap.GetString("JobName") ?? "Unknown";
            string lastRunFile = context.JobDetail.JobDataMap.GetString("LastRunFilePath") ?? "job_schedule_history.json";

            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Executing job: {jobName}");
            await UpdateLastRunAsync(jobName, lastRunFile);
        }

        public async Task RunManually(string jobName, string filePath)
        {
            Console.WriteLine($"[MANUAL] Running missed job: {jobName} at {DateTimeOffset.UtcNow:O}");
            await UpdateLastRunAsync(jobName, filePath);
        }

        private async Task UpdateLastRunAsync(string jobName, string filePath)
        {
            List<LastRunEntry> entries = new();

            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrWhiteSpace(json))
                    entries = new();
                else
                {
                    entries = JsonSerializer.Deserialize<List<LastRunEntry>>(json) ?? new();
                }
            }

            var existing = entries.FirstOrDefault(e => e.JobName == jobName);
            if (existing != null)
            {
                existing.LastRun = DateTimeOffset.UtcNow;
            }
            else
            {
                entries.Add(new LastRunEntry
                {
                    JobName = jobName,
                    LastRun = DateTimeOffset.UtcNow
                });
            }

            var updatedJson = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, updatedJson);
        }
    }
}
