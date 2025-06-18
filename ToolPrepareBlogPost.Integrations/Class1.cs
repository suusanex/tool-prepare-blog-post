namespace ToolPrepareBlogPost.Integrations;

// Qiita APIインターフェース
public interface IQiitaApiClient
{
    Task<QiitaArticle?> GetLatestArticleAsync(string userId, CancellationToken cancellationToken = default);
    Task<QiitaArticle?> GetArticleByIdAsync(string articleId, CancellationToken cancellationToken = default);
}

public class QiitaArticle
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Markdown { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

// Zenn CLI/リポジトリインターフェース
public interface IZennDraftService
{
    Task<string> CreateDraftAsync(QiitaArticle article, string userId, CancellationToken cancellationToken = default);
}

// はてなブログ AtomPub APIインターフェース
public interface IHatenaBlogDraftService
{
    Task<string> CreateDraftAsync(string zennArticleUrl, string template, string userId, CancellationToken cancellationToken = default);
}

// Webhook通知インターフェース
public interface IWebhookNotifier
{
    Task NotifyAsync(string userId, string message, CancellationToken cancellationToken = default);
}

// テンプレート管理インターフェース
public interface ITemplateProvider
{
    Task<string> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default);
}

// LinkedIn文面生成インターフェース
public interface ILinkedInMessageGenerator
{
    Task<string> GenerateMessageAsync(string qiitaArticleUrl, string template, CancellationToken cancellationToken = default);
}

// --- 以下スタブ実装 ---
public class DummyQiitaApiClient : IQiitaApiClient
{
    public Task<QiitaArticle?> GetLatestArticleAsync(string userId, CancellationToken cancellationToken = default)
        => Task.FromResult<QiitaArticle?>(new QiitaArticle { Id = "1", Title = "Dummy", Markdown = "# Dummy", Url = "https://qiita.com/dummy", CreatedAt = DateTimeOffset.UtcNow });
    public Task<QiitaArticle?> GetArticleByIdAsync(string articleId, CancellationToken cancellationToken = default)
        => Task.FromResult<QiitaArticle?>(new QiitaArticle { Id = articleId, Title = "Dummy", Markdown = "# Dummy", Url = $"https://qiita.com/dummy/{articleId}", CreatedAt = DateTimeOffset.UtcNow });
}

public class DummyZennDraftService : IZennDraftService
{
    public Task<string> CreateDraftAsync(QiitaArticle article, string userId, CancellationToken cancellationToken = default)
        => Task.FromResult($"https://zenn.dev/{userId}/drafts/{article.Id}");
}

public class DummyHatenaBlogDraftService : IHatenaBlogDraftService
{
    public Task<string> CreateDraftAsync(string zennArticleUrl, string template, string userId, CancellationToken cancellationToken = default)
        => Task.FromResult($"https://hatenablog.com/{userId}/drafts/{Guid.NewGuid()}");
}

public class DummyWebhookNotifier : IWebhookNotifier
{
    public Task NotifyAsync(string userId, string message, CancellationToken cancellationToken = default)
    {
        System.Console.WriteLine($"[Webhook通知] userId={userId}, message={message}");
        return Task.CompletedTask;
    }
}

public class DummyTemplateProvider : ITemplateProvider
{
    public Task<string> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default)
        => Task.FromResult($"テンプレート:{templateName}");
}

public class DummyLinkedInMessageGenerator : ILinkedInMessageGenerator
{
    public Task<string> GenerateMessageAsync(string qiitaArticleUrl, string template, CancellationToken cancellationToken = default)
        => Task.FromResult($"{template} (from {qiitaArticleUrl})");
}
