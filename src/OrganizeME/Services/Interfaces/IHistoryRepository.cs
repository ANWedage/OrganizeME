using FolderFlow.Models;

namespace FolderFlow.Services.Interfaces;

/// <summary>
/// Persists and retrieves file-organizing history records using SQLite.
/// </summary>
public interface IHistoryRepository
{
    Task InitializeAsync();

    Task AddEntryAsync(FileHistoryEntry entry);

    Task<List<FileHistoryEntry>> GetRecentAsync(int count = 200);

    Task ClearAllAsync();
}
