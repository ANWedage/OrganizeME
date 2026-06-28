using System.IO;
using Microsoft.Data.Sqlite;

namespace FolderFlow.Data;

/// <summary>
/// Central place for the SQLite connection string and schema creation.
/// The database lives in the user's AppData folder so it persists across
/// app updates and doesn't require admin rights to write to.
/// </summary>
public static class DatabaseConfig
{
    public static string DatabaseFolder
    {
        get
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "OrganizeME");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }

    public static string DatabasePath => Path.Combine(DatabaseFolder, "organizeme.db");

    public static string ConnectionString => $"Data Source={DatabasePath}";

    public static async Task EnsureSchemaAsync()
    {
        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS FileHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OriginalPath TEXT NOT NULL,
                NewPath TEXT NOT NULL,
                FileName TEXT NOT NULL,
                Extension TEXT NOT NULL,
                Category TEXT NOT NULL,
                Timestamp TEXT NOT NULL,
                AiUsed INTEGER NOT NULL,
                Success INTEGER NOT NULL,
                ErrorMessage TEXT NULL
            );
            """;

        await command.ExecuteNonQueryAsync();
    }
}
