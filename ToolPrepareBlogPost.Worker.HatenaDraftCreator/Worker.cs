using ToolPrepareBlogPost.Integrations;

namespace ToolPrepareBlogPost.Worker.HatenaDraftCreator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    // ����Worker��ZennWatcher����̃C�x���g�ŋN������z��̂��߁A������ZennWatcher�ɏW��\
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("HatenaDraftCreator Worker is idle. (�@�\��ZennWatcher�ɓ���)");
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
