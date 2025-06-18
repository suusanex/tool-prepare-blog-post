using ToolPrepareBlogPost.Worker.QiitaWatcher;
using ToolPrepareBlogPost.Integrations;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
// �_�~�[������DI�o�^
builder.Services.AddSingleton<IQiitaApiClient, DummyQiitaApiClient>();
builder.Services.AddSingleton<IZennDraftService, DummyZennDraftService>();
builder.Services.AddSingleton<IWebhookNotifier, DummyWebhookNotifier>();
builder.Services.AddSingleton<ILinkedInMessageGenerator, DummyLinkedInMessageGenerator>();
builder.Services.AddSingleton<ITemplateProvider, DummyTemplateProvider>();

var host = builder.Build();
host.Run();
