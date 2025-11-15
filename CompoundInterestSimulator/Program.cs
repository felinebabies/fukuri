using Quartz;
using Quartz.Impl;

namespace CompoundInterestSimulator;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class CompoundInterestJob : IJob
{
    public const decimal InterestRatePerInterval = 0.015m;

    public Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var currentDebt = (decimal)dataMap["currentDebt"];
        var previousDebt = currentDebt;

        currentDebt *= 1m + InterestRatePerInterval;
        dataMap["currentDebt"] = currentDebt;

        Console.WriteLine(
            $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} 現在の借金額: {currentDebt:N0} 円 (前回からの増加: {(currentDebt - previousDebt):N0} 円)"
        );

        return Task.CompletedTask;
    }
}

public static class Program
{
    private const decimal InitialDebt = 10_000_000m;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

    public static async Task Main(string[] args)
    {
        using var cancellationSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationSource.Cancel();
        };

        var schedulerFactory = new StdSchedulerFactory();
        var scheduler = await schedulerFactory.GetScheduler();

        var jobData = new JobDataMap
        {
            { "currentDebt", InitialDebt }
        };

        var job = JobBuilder.Create<CompoundInterestJob>()
            .WithIdentity("compoundInterestJob", "debt")
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("compoundInterestTrigger", "debt")
            .StartAt(DateBuilder.NextGivenSecondDate(DateTimeOffset.UtcNow.Add(Interval), 0)) 
            .WithSimpleSchedule(schedule => schedule
                .WithInterval(Interval)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);
        await scheduler.Start();

        Console.WriteLine("複利計算シミュレーターを開始しました。Ctrl+Cで終了します。");
        Console.WriteLine(
            $"初期借金額: {InitialDebt:N0} 円 / 複利間隔: {Interval.TotalMinutes} 分毎 / 利率: {CompoundInterestJob.InterestRatePerInterval * 100m:0.###}%"
        );

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation.
        }

        await scheduler.Shutdown();
        Console.WriteLine("シミュレーターを終了しました。");
    }
}
