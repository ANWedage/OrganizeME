using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using FolderFlow.Services.Interfaces;
using Serilog;

namespace FolderFlow.Services;

public sealed class UpdateService : IUpdateService
{
    // -----------------------------------------------------------------------
    // TODO: Replace these two constants with your actual GitHub owner/repo.
    // -----------------------------------------------------------------------
    private const string GitHubOwner = "ANWedage";
    private const string GitHubRepo  = "OrganizeME";
    // -----------------------------------------------------------------------

    private static readonly HttpClient _http = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "OrganizeME-UpdateChecker" },
            { "Accept",     "application/vnd.github+json" }
        },
        Timeout = TimeSpan.FromSeconds(15)
    };

    private readonly ILogger _logger;

    public Version CurrentVersion { get; } =
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

    public UpdateService(ILogger logger) => _logger = logger;

    public async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        try
        {
            var url = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
            var release = await _http.GetFromJsonAsync<GitHubRelease>(url);

            if (release is null || string.IsNullOrWhiteSpace(release.TagName))
                return null;

            // Tag is expected to be "v1.2.3" or "1.2.3"
            var tagVersion = release.TagName.TrimStart('v', 'V');
            if (!Version.TryParse(tagVersion, out var remoteVersion))
                return null;

            if (remoteVersion <= CurrentVersion)
                return null;

            // Pick the first .exe or .msi asset as the installer
            var asset = release.Assets?.FirstOrDefault(a =>
                a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                a.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase));

            var downloadUrl = asset?.BrowserDownloadUrl ?? release.HtmlUrl;
            var fileName    = asset?.Name ?? $"OrganizeME-{release.TagName}.exe";

            return new UpdateInfo(remoteVersion, release.TagName, release.Body ?? string.Empty, downloadUrl, fileName);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Update check failed.");
            return null;
        }
    }

    public async Task DownloadAndInstallAsync(UpdateInfo update, IProgress<int> progress, CancellationToken ct = default)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), update.FileName);

        using var response = await _http.GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1L;
        await using var dest = File.Create(tempPath);
        await using var src  = await response.Content.ReadAsStreamAsync(ct);

        var buffer    = new byte[81920];
        long received = 0;
        int  read;

        while ((read = await src.ReadAsync(buffer, ct)) > 0)
        {
            await dest.WriteAsync(buffer.AsMemory(0, read), ct);
            received += read;
            if (total > 0)
                progress.Report((int)(received * 100 / total));
        }

        dest.Close();

        _logger.Information("Update downloaded to {Path}. Launching installer.", tempPath);
        Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
    }

    // ── GitHub JSON models ───────────────────────────────────────────────────

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]   public string?              TagName { get; set; }
        [JsonPropertyName("html_url")]   public string               HtmlUrl { get; set; } = string.Empty;
        [JsonPropertyName("body")]       public string?              Body    { get; set; }
        [JsonPropertyName("assets")]     public List<GitHubAsset>?   Assets  { get; set; }
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]                  public string Name                 { get; set; } = string.Empty;
        [JsonPropertyName("browser_download_url")] public string BrowserDownloadUrl   { get; set; } = string.Empty;
    }
}
