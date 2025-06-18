using ToolPrepareBlogPost.Integrations;

namespace ToolPrepareBlogPost.Worker.HatenaDraftCreator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    // このWorkerはZennWatcherからのイベントで起動する想定のため、実装はZennWatcherに集約可能
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("HatenaDraftCreator Worker is idle. (機能はZennWatcherに統合)");
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
