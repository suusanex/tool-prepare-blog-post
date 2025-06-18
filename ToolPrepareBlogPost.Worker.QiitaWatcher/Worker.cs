using ToolPrepareBlogPost.Integrations;

namespace ToolPrepareBlogPost.Worker.QiitaWatcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IQiitaApiClient _qiitaApiClient;
    private readonly IZennDraftService _zennDraftService;
    private readonly IWebhookNotifier _webhookNotifier;
    private readonly ILinkedInMessageGenerator _linkedInMessageGenerator;
    private readonly ITemplateProvider _templateProvider;
    private readonly string _userId = "your-user-id"; // TODO: ���[�U�[ID�̎擾���@������

    public Worker(
        ILogger<Worker> logger,
        IQiitaApiClient qiitaApiClient,
        IZennDraftService zennDraftService,
        IWebhookNotifier webhookNotifier,
        ILinkedInMessageGenerator linkedInMessageGenerator,
        ITemplateProvider templateProvider)
    {
        _logger = logger;
        _qiitaApiClient = qiitaApiClient;
        _zennDraftService = zennDraftService;
        _webhookNotifier = webhookNotifier;
        _linkedInMessageGenerator = linkedInMessageGenerator;
        _templateProvider = templateProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. Qiita�̍ŐV�L�����擾
                var article = await _qiitaApiClient.GetLatestArticleAsync(_userId, stoppingToken);
                if (article != null)
                {
                    // 2. Zenn�������쐬
                    var zennDraftUrl = await _zennDraftService.CreateDraftAsync(article, _userId, stoppingToken);
                    await _webhookNotifier.NotifyAsync(_userId, $"Zenn�������쐬����: {zennDraftUrl}", stoppingToken);

                    // 3. LinkedIn���ʐ���
                    var template = await _templateProvider.GetTemplateAsync("LinkedIn", stoppingToken);
                    var message = await _linkedInMessageGenerator.GenerateMessageAsync(article.Url, template, stoppingToken);
                    await _webhookNotifier.NotifyAsync(_userId, $"LinkedIn���e����: {message}", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�G���[���������܂���");
            }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // �|�[�����O�Ԋu
        }
    }
}
