using ToolPrepareBlogPost.Worker.HatenaDraftCreator;
using ToolPrepareBlogPost.Integrations;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
// �K�v�ɉ����ă_�~�[������DI�o�^�i����͖��g�p�j

var host = builder.Build();
host.Run();
