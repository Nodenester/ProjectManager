using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectManager.Services;

public interface IGitHubService
{
    Task<IEnumerable<GitHubRepo>> GetUserRepositoriesAsync(string accessToken);
    Task<GitHubUser> GetUserAsync(string accessToken);
    Task<IEnumerable<GitHubCommit>> GetRepositoryCommitsAsync(string accessToken, string repoName);
    Task<IEnumerable<GitHubContributor>> GetRepositoryContributorsAsync(string accessToken, string repoName);
    Task<GitHubRepoDetails> GetRepositoryDetailsAsync(string accessToken, string repoName);
    Task<IEnumerable<GitHubContent>> GetRepositoryContentsAsync(string accessToken, string repoName, string? path = null);
}

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://api.github.com";

    public GitHubService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("ProjectManager", "1.0"));
    }

    private void SetAuthHeader(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<IEnumerable<GitHubRepo>> GetUserRepositoriesAsync(string accessToken)
    {
        SetAuthHeader(accessToken);
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/user/repos");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<GitHubRepo>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? Enumerable.Empty<GitHubRepo>();
    }

    public async Task<GitHubUser> GetUserAsync(string accessToken)
    {
        SetAuthHeader(accessToken);
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/user");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GitHubUser>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new Exception("Failed to get user info");
    }

    public async Task<IEnumerable<GitHubCommit>> GetRepositoryCommitsAsync(string accessToken, string repoName)
    {
        SetAuthHeader(accessToken);
        var user = await GetUserAsync(accessToken);
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/repos/{user.Login}/{repoName}/commits");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<GitHubCommit>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? Enumerable.Empty<GitHubCommit>();
    }

    public async Task<IEnumerable<GitHubContributor>> GetRepositoryContributorsAsync(string accessToken, string repoName)
    {
        SetAuthHeader(accessToken);
        var user = await GetUserAsync(accessToken);
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/repos/{user.Login}/{repoName}/contributors");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<GitHubContributor>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? Enumerable.Empty<GitHubContributor>();
    }

    public async Task<GitHubRepoDetails> GetRepositoryDetailsAsync(string accessToken, string repoName)
    {
        SetAuthHeader(accessToken);
        var user = await GetUserAsync(accessToken);
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/repos/{user.Login}/{repoName}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"GitHub API Response: {content}"); // For debugging

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Deserialize<GitHubRepoDetails>(content, options)
            ?? throw new Exception("Failed to get repository details");
    }

    public async Task<IEnumerable<GitHubContent>> GetRepositoryContentsAsync(
        string accessToken,
        string repoName,
        string? path = null)
    {
        SetAuthHeader(accessToken);
        var user = await GetUserAsync(accessToken);

        var requestUrl = $"{ApiBaseUrl}/repos/{user.Login}/{repoName}/contents";
        if (!string.IsNullOrEmpty(path))
        {
            requestUrl += $"/{path}";
        }

        var response = await _httpClient.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // GitHub API returns either a single object or an array depending on whether
        // we're requesting a single file or a directory
        try
        {
            var contentArray = JsonSerializer.Deserialize<IEnumerable<GitHubContent>>(content, options);
            return contentArray ?? Enumerable.Empty<GitHubContent>();
        }
        catch
        {
            // If it's a single file
            var singleContent = JsonSerializer.Deserialize<GitHubContent>(content, options);
            return singleContent != null ? new[] { singleContent } : Enumerable.Empty<GitHubContent>();
        }
    }
}

// Model classes
public record GitHubRepo(
    int Id,
    string Name,
    string FullName,
    string Description,
    string HtmlUrl,
    bool Private
);

public record GitHubUser(
    string Login,
    int Id,
    string AvatarUrl,
    string HtmlUrl,
    string Name,
    string Email
);

public record GitHubCommit(
    string Sha,
    GitHubCommitDetails Commit,
    GitHubAuthor Author,
    DateTimeOffset Date
);

public record GitHubCommitDetails(
    GitHubCommitAuthor Author,
    string Message
);

public record GitHubCommitAuthor(
    string Name,
    string Email,
    DateTimeOffset Date
);

public record GitHubAuthor(
    string Login,
    string AvatarUrl,
    string HtmlUrl
);

public record GitHubContributor(
    string Login,
    string AvatarUrl,
    string HtmlUrl,
    int Contributions
);

public record GitHubRepoDetails(
    int Id,
    string Name,
    string FullName,
    string Description,
    string HtmlUrl,
    bool Private,
    [property: JsonPropertyName("stargazers_count")] int StargazersCount,
    [property: JsonPropertyName("forks_count")] int ForksCount,
    [property: JsonPropertyName("open_issues_count")] int OpenIssuesCount,
    [property: JsonPropertyName("watchers_count")] int WatchersCount,
    [property: JsonPropertyName("default_branch")] string DefaultBranch,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt,
    [property: JsonPropertyName("pushed_at")] DateTimeOffset PushedAt,
    GitHubLicense? License
);

public record GitHubLicense(
    string Key,
    string Name,
    string Url
);

public record GitHubContent(
    string Name,
    string Path,
    string Sha,
    long Size,
    string Type,  // "file" or "dir"
    string Url,
    string HtmlUrl,
    string DownloadUrl,
    string? Content = null
);