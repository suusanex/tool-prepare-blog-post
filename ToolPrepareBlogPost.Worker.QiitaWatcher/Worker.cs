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
    private readonly string _userId = "your-user-id"; // TODO: ユーザーIDの取得方法を実装

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
                // 1. Qiitaの最新記事を取得
                var article = await _qiitaApiClient.GetLatestArticleAsync(_userId, stoppingToken);
                if (article != null)
                {
                    // 2. Zenn下書き作成
                    var zennDraftUrl = await _zennDraftService.CreateDraftAsync(article, _userId, stoppingToken);
                    await _webhookNotifier.NotifyAsync(_userId, $"Zenn下書き作成完了: {zennDraftUrl}", stoppingToken);

                    // 3. LinkedIn文面生成
                    var template = await _templateProvider.GetTemplateAsync("LinkedIn", stoppingToken);
                    var message = await _linkedInMessageGenerator.GenerateMessageAsync(article.Url, template, stoppingToken);
                    await _webhookNotifier.NotifyAsync(_userId, $"LinkedIn投稿文面: {message}", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "エラーが発生しました");
            }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // ポーリング間隔
        }
    }
}
