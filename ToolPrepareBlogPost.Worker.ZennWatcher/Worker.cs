using ToolPrepareBlogPost.Integrations;

namespace ToolPrepareBlogPost.Worker.ZennWatcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHatenaBlogDraftService _hatenaBlogDraftService;
    private readonly IWebhookNotifier _webhookNotifier;
    private readonly ITemplateProvider _templateProvider;
    private readonly string _userId = "your-user-id"; // TODO: ���[�U�[ID�̎擾���@������

    public Worker(
        ILogger<Worker> logger,
        IHatenaBlogDraftService hatenaBlogDraftService,
        IWebhookNotifier webhookNotifier,
        ITemplateProvider templateProvider)
    {
        _logger = logger;
        _hatenaBlogDraftService = hatenaBlogDraftService;
        _webhookNotifier = webhookNotifier;
        _templateProvider = templateProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // TODO: Zenn���e�C�x���g�̎�M����������
                string zennArticleUrl = "https://zenn.dev/your-article-url"; // ��
                var template = await _templateProvider.GetTemplateAsync("HatenaBlog", stoppingToken);
                var hatenaDraftUrl = await _hatenaBlogDraftService.CreateDraftAsync(zennArticleUrl, template, _userId, stoppingToken);
                await _webhookNotifier.NotifyAsync(_userId, $"�͂Ăȃu���O�������쐬����: {hatenaDraftUrl}", stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�G���[���������܂���");
            }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // ���̃|�[�����O�Ԋu
        }
    }
}
