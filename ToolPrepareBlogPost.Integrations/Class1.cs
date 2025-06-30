using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

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

public class ZennDraftService : IZennDraftService
{
    private readonly string _repoPath;
    private readonly string _gitUserName;
    private readonly string _gitUserEmail;
    private readonly string _gitToken;

    public ZennDraftService(string repoPath, string gitUserName, string gitUserEmail, string gitToken)
    {
        _repoPath = repoPath;
        _gitUserName = gitUserName;
        _gitUserEmail = gitUserEmail;
        _gitToken = gitToken;
    }

    public async Task<string> CreateDraftAsync(QiitaArticle article, string userId, CancellationToken cancellationToken = default)
    {
        // 1. slug生成（簡易: タイトルのスラッグ化）
        var slug = GenerateSlug(article.Title);
        var articleDir = Path.Combine(_repoPath, "articles");
        Directory.CreateDirectory(articleDir);
        var articlePath = Path.Combine(articleDir, $"{slug}.md");

        // 2. YAMLヘッダ＋Markdown本文を生成
        var yaml = $"---\ntitle: '{EscapeYaml(article.Title)}'\ntype: 'tech'\npublished: false\n---\n";
        var content = yaml + article.Markdown;
        await File.WriteAllTextAsync(articlePath, content, Encoding.UTF8, cancellationToken);

        // 3. git add/commit/push
        RunGit($"config user.name \"{_gitUserName}\"", _repoPath);
        RunGit($"config user.email \"{_gitUserEmail}\"", _repoPath);
        RunGit($"add articles/{slug}.md", _repoPath);
        RunGit($"commit -m \"Add Zenn draft: {article.Title}\"", _repoPath);
        // push with token
        var originUrl = RunGitWithOutput("remote get-url origin", _repoPath).Trim();
        var urlWithToken = InsertTokenToGitUrl(originUrl, _gitToken);
        RunGit($"remote set-url origin {urlWithToken}", _repoPath);
        RunGit("push origin HEAD", _repoPath);
        // 戻す
        RunGit($"remote set-url origin {originUrl}", _repoPath);

        // 4. Zenn下書きURL生成
        return $"https://zenn.dev/{userId}/articles/{slug}";
    }

    private static string GenerateSlug(string title)
    {
        var sb = new StringBuilder();
        foreach (var c in title.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (char.IsWhiteSpace(c) || c == '-') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }

    private static string EscapeYaml(string input)
        => input.Replace("'", "''");

    private static void RunGit(string args, string workingDir)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        using var proc = Process.Start(psi);
        proc.WaitForExit();
        if (proc.ExitCode != 0)
            throw new Exception($"git {args} failed: {proc.StandardError.ReadToEnd()}");
    }

    private static string RunGitWithOutput(string args, string workingDir)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        using var proc = Process.Start(psi);
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        if (proc.ExitCode != 0)
            throw new Exception($"git {args} failed: {proc.StandardError.ReadToEnd()}");
        return output;
    }

    private static string InsertTokenToGitUrl(string url, string token)
    {
        if (url.StartsWith("https://"))
        {
            var idx = "https://".Length;
            return url.Insert(idx, $"{token}@");
        }
        return url;
    }
}

public class QiitaApiClient : IQiitaApiClient
{
    private readonly string _accessToken;
    private readonly HttpClient _httpClient;

    public QiitaApiClient(string accessToken, HttpClient? httpClient = null)
    {
        _accessToken = accessToken;
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.BaseAddress = new Uri("https://qiita.com/");
    }

    public async Task<QiitaArticle?> GetLatestArticleAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Qiita API: GET /api/v2/items?query=user:{userId}&per_page=1&sort=created
        var url = $"api/v2/items?query=user:{userId}&per_page=1&sort=created";
        var res = await _httpClient.GetAsync(url, cancellationToken);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync(cancellationToken);
        var items = JsonSerializer.Deserialize<List<QiitaItemDto>>(json);
        if (items == null || items.Count == 0) return null;
        return ToArticle(items[0]);
    }

    public async Task<QiitaArticle?> GetArticleByIdAsync(string articleId, CancellationToken cancellationToken = default)
    {
        var url = $"api/v2/items/{articleId}";
        var res = await _httpClient.GetAsync(url, cancellationToken);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync(cancellationToken);
        var item = JsonSerializer.Deserialize<QiitaItemDto>(json);
        return item == null ? null : ToArticle(item);
    }

    private static QiitaArticle ToArticle(QiitaItemDto dto)
        => new QiitaArticle
        {
            Id = dto.id,
            Title = dto.title,
            Markdown = dto.body,
            Url = dto.url,
            CreatedAt = dto.created_at
        };

    private class QiitaItemDto
    {
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string body { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
        public DateTimeOffset created_at { get; set; }
    }
}

public class WebhookNotifier : IWebhookNotifier
{
    private readonly string _notifyUrl;
    private readonly HttpClient _httpClient;

    public WebhookNotifier(IConfiguration configuration, HttpClient? httpClient = null)
    {
        _notifyUrl = configuration.GetSection("Webhook")["NotifyUrl"] ?? throw new ArgumentException("Webhook:NotifyUrl is required");
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task NotifyAsync(string userId, string message, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            userId,
            message
        };
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var res = await _httpClient.PostAsync(_notifyUrl, content, cancellationToken);
        res.EnsureSuccessStatusCode();
    }
}
