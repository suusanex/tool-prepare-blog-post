using ToolPrepareBlogPost.Worker.ZennWatcher;
using ToolPrepareBlogPost.Integrations;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
// ƒ_ƒ~[À‘•‚ğDI“o˜^
builder.Services.AddSingleton<IHatenaBlogDraftService, DummyHatenaBlogDraftService>();
builder.Services.AddSingleton<IWebhookNotifier, DummyWebhookNotifier>();
builder.Services.AddSingleton<ITemplateProvider, DummyTemplateProvider>();

var host = builder.Build();
host.Run();
