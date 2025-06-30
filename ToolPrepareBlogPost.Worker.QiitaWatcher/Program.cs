using ToolPrepareBlogPost.Worker.QiitaWatcher;
using ToolPrepareBlogPost.Integrations;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// appsettings.jsonからZenn設定を取得
var zennConfig = builder.Configuration.GetSection("Zenn");
var repoPath = zennConfig["GitHubRepoPath"] ?? "";
var gitUserName = zennConfig["GitHubUserName"] ?? "";
var gitUserEmail = zennConfig["GitHubEmail"] ?? "";
var gitToken = zennConfig["GitHubToken"] ?? "";

// appsettings.jsonからQiita設定を取得
var qiitaConfig = builder.Configuration.GetSection("QiitaApi");
var qiitaAccessToken = qiitaConfig["AccessToken"] ?? "";

builder.Services.AddSingleton<IQiitaApiClient>(
    _ => new QiitaApiClient(qiitaAccessToken));
builder.Services.AddSingleton<IZennDraftService>(
    _ => new ZennDraftService(repoPath, gitUserName, gitUserEmail, gitToken));
builder.Services.AddSingleton<IWebhookNotifier>(
    sp => new WebhookNotifier(builder.Configuration));
builder.Services.AddSingleton<ILinkedInMessageGenerator, DummyLinkedInMessageGenerator>();
builder.Services.AddSingleton<ITemplateProvider, DummyTemplateProvider>();

var host = builder.Build();
host.Run();
