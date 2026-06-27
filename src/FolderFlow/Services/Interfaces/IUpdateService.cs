namespace FolderFlow.Services.Interfaces;

public interface IUpdateService
{
    /// <summary>Current application version.</summary>
    Version CurrentVersion { get; }

    /// <summary>Checks GitHub releases. Returns null if already up-to-date or check failed.</summary>
    Task<UpdateInfo?> CheckForUpdateAsync();

    /// <summary>Downloads the installer asset to %TEMP% and launches it.</summary>
    Task DownloadAndInstallAsync(UpdateInfo update, IProgress<int> progress, CancellationToken ct = default);
}

public sealed record UpdateInfo(Version Version, string TagName, string ReleaseNotes, string DownloadUrl, string FileName);
