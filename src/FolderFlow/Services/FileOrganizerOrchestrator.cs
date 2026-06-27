using System.IO;
using FolderFlow.Models;
using FolderFlow.Services.Interfaces;
using Serilog;

namespace FolderFlow.Services;

public class FileOrganizerOrchestrator : IFileOrganizerOrchestrator
{
    private readonly IFileCategorizer _categorizer;
    private readonly IFileMoverService _mover;
    private readonly IHistoryRepository _history;
    private readonly INotificationService _notifications;
    private readonly ISettingsService _settings;
    private readonly ILogger _logger;

    private const string FallbackCategory = "Other";

    public FileOrganizerOrchestrator(
        IFileCategorizer categorizer,
        IFileMoverService mover,
        IHistoryRepository history,
        INotificationService notifications,
        ISettingsService settings,
        ILogger logger)
    {
        _categorizer = categorizer;
        _mover = mover;
        _history = history;
        _notifications = notifications;
        _settings = settings;
        _logger = logger.ForContext<FileOrganizerOrchestrator>();
    }

    public async Task ProcessNewFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var extension = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
        var settings = _settings.Current;

        var entry = new FileHistoryEntry
        {
            OriginalPath = filePath,
            FileName = fileName,
            Extension = extension,
            AiUsed = false, // V2 will set this when the AI categorizer is used
        };

        try
        {
            var category = _categorizer.Categorize(filePath, settings.Rules) ?? FallbackCategory;
            entry.Category = category;

            if (!settings.AutoMoveEnabled)
            {
                // Dry-run mode: log what would happen but don't touch the file.
                entry.NewPath = filePath;
                entry.Success = true;
                entry.ErrorMessage = "Auto-move disabled (dry run) — file left in place.";
                _logger.Information("Dry run: {File} would go to {Category}", fileName, category);
            }
            else
            {
                var destination = await _mover.MoveFileAsync(filePath, settings.OrganizedRootPath, category);
                entry.NewPath = destination;
                entry.Success = true;

                _notifications.ShowFileOrganized(fileName, category);
                _logger.Information("Organized {File} into {Category}", fileName, category);
            }
        }
        catch (Exception ex)
        {
            entry.Success = false;
            entry.ErrorMessage = ex.Message;
            entry.NewPath = filePath;

            _notifications.ShowError($"Couldn't organize \"{fileName}\": {ex.Message}");
            _logger.Error(ex, "Failed to organize {File}", fileName);
        }

        await _history.AddEntryAsync(entry);
    }

    public async Task OrganizeExistingFilesAsync(string folderPath, IProgress<(int done, int total)>? progress = null, CancellationToken ct = default)
    {
        var files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
        var total = files.Length;

        for (int i = 0; i < total; i++)
        {
            ct.ThrowIfCancellationRequested();
            await ProcessNewFileAsync(files[i]);
            progress?.Report((i + 1, total));
        }
    }
}
