using MoYu.Schedule;
using Microsoft.Extensions.Logging;

namespace MoYu.Application;

//[Period(10000)]
[At("2024-12-31 23:59:59")]
public class TestJob : IJob, IDisposable
{
    private readonly ILogger<TestJob> _logger;
    public TestJob(ILogger<TestJob> logger)
    {
        _logger = logger;
    }

    public void Dispose()
    {
        Console.WriteLine("释放了");
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        _logger.LogWarning($"{context}");
        await Task.CompletedTask;
    }
}