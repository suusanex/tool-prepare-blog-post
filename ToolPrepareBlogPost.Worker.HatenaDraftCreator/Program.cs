using ToolPrepareBlogPost.Worker.HatenaDraftCreator;
using ToolPrepareBlogPost.Integrations;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
// 必要に応じてダミー実装をDI登録（現状は未使用）

var host = builder.Build();
host.Run();
