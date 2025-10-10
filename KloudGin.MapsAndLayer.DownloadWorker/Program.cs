using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.AspNetCore;
using RestSharp;

try
{
    // Ensure Logs folder exists
    var logsDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
    Directory.CreateDirectory(logsDirectory);

    // Use an exact filename containing the current date (UTC) so every run on the same day
    // targets the same file. Serilog's file sink opens in append mode by default, so
    // new runs will append to the same day's file rather than creating another file.
    var logFilePath = Path.Combine(logsDirectory, $"log-{DateTime.UtcNow:yyyy-MM-dd}.txt");

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.File(
            path: logFilePath,
            // keep RollingInterval infinite because filename already includes the date
            rollingInterval: RollingInterval.Infinite,
            retainedFileCountLimit: 31,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        )
        .CreateLogger();

    Log.Information("Starting host");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog() // plug Serilog into Microsoft.Extensions.Logging
        .ConfigureServices((context, services) =>
        { 
            services.AddSingleton(new RestClient());
            services.AddHostedService<Worker>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
