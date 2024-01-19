using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace BackPot.Client;

internal abstract class Program
{
    public static Configuration Configuration { get; } = Configuration.GetConfiguration();
    static async void Main()
    {
        LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
        var scheduler = await StdSchedulerFactory.GetDefaultScheduler();

        var backupJob = JobBuilder.Create<BackupJob>()
            .WithIdentity("backupJob", "backpot")
            .DisallowConcurrentExecution()
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("backupTrigger", "backpot")
            .WithCronSchedule(Configuration.Cron)
            .Build();

        await scheduler.ScheduleJob(backupJob, trigger);
        await scheduler.Start();

        await Task.Delay(-1);
    }
}