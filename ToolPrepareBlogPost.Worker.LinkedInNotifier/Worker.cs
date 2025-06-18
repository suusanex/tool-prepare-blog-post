using ToolPrepareBlogPost.Integrations;

namespace ToolPrepareBlogPost.Worker.LinkedInNotifier;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    // ����Worker��QiitaWatcher����̃C�x���g�ŋN������z��̂��߁A������QiitaWatcher�ɏW��\
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("LinkedInNotifier Worker is idle. (�@�\��QiitaWatcher�ɓ���)");
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
