using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Quartz.Simpl;
using KGWin.Schdeuler;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddQuartz();
        services.AddSingleton<IJobFactory, SimpleJobFactory>();
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
        services.AddHostedService<Worker>();
    })
    .Build()
    .Run();
