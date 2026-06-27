using Dapper;
using FolderFlow.Data;
using FolderFlow.Models;
using FolderFlow.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Serilog;

namespace FolderFlow.Services;

public class SqliteHistoryRepository : IHistoryRepository
{
    private readonly ILogger _logger;

    public SqliteHistoryRepository(ILogger logger)
    {
        _logger = logger.ForContext<SqliteHistoryRepository>();
    }

    public async Task InitializeAsync()
    {
        await DatabaseConfig.EnsureSchemaAsync();
        _logger.Information("History database ready at {Path}", DatabaseConfig.DatabasePath);
    }

    public async Task AddEntryAsync(FileHistoryEntry entry)
    {
        await using var connection = new SqliteConnection(DatabaseConfig.ConnectionString);
        await connection.OpenAsync();

        const string sql = """
            INSERT INTO FileHistory
                (OriginalPath, NewPath, FileName, Extension, Category, Timestamp, AiUsed, Success, ErrorMessage)
            VALUES
                (@OriginalPath, @NewPath, @FileName, @Extension, @Category, @Timestamp, @AiUsed, @Success, @ErrorMessage);
            """;

        await connection.ExecuteAsync(sql, new
        {
            entry.OriginalPath,
            entry.NewPath,
            entry.FileName,
            entry.Extension,
            entry.Category,
            Timestamp = entry.Timestamp.ToString("O"),
            AiUsed = entry.AiUsed ? 1 : 0,
            Success = entry.Success ? 1 : 0,
            entry.ErrorMessage
        });
    }

    public async Task<List<FileHistoryEntry>> GetRecentAsync(int count = 200)
    {
        await using var connection = new SqliteConnection(DatabaseConfig.ConnectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT Id, OriginalPath, NewPath, FileName, Extension, Category,
                   Timestamp, AiUsed, Success, ErrorMessage
            FROM FileHistory
            ORDER BY Id DESC
            LIMIT @count;
            """;

        var rows = await connection.QueryAsync(sql, new { count });

        var results = new List<FileHistoryEntry>();
        foreach (var row in rows)
        {
            results.Add(new FileHistoryEntry
            {
                Id = (int)(long)row.Id,
                OriginalPath = row.OriginalPath,
                NewPath = row.NewPath,
                FileName = row.FileName,
                Extension = row.Extension,
                Category = row.Category,
                Timestamp = DateTime.Parse(row.Timestamp),
                AiUsed = (long)row.AiUsed == 1,
                Success = (long)row.Success == 1,
                ErrorMessage = row.ErrorMessage
            });
        }

        return results;
    }

    public async Task ClearAllAsync()
    {
        await using var connection = new SqliteConnection(DatabaseConfig.ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("DELETE FROM FileHistory;");
        _logger.Information("History cleared by user.");
    }
}
