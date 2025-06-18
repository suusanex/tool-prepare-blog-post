using ToolPrepareBlogPost.Integrations;

namespace ToolPrepareBlogPost.Worker.LinkedInNotifier;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    // このWorkerはQiitaWatcherからのイベントで起動する想定のため、実装はQiitaWatcherに集約可能
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("LinkedInNotifier Worker is idle. (機能はQiitaWatcherに統合)");
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
