namespace Swg.Capture;

internal static class SqliteSchema
{
    public const int CurrentVersion = 1;

    /// <summary>初始化库：WAL、schema_version、http_exchange 及索引。</summary>
    public static void Apply(Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            PRAGMA journal_mode=WAL;
            PRAGMA synchronous=NORMAL;
            CREATE TABLE IF NOT EXISTS schema_version (
              id INTEGER PRIMARY KEY CHECK (id = 1),
              version INTEGER NOT NULL
            );
            INSERT OR IGNORE INTO schema_version (id, version) VALUES (1, 0);
            """;
        cmd.ExecuteNonQuery();

        int version = GetSchemaVersion(connection);
        if (version < 1)
            MigrateToV1(connection);

        cmd.CommandText = "UPDATE schema_version SET version = 1 WHERE id = 1;";
        cmd.ExecuteNonQuery();
    }

    private static int GetSchemaVersion(Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT version FROM schema_version WHERE id = 1;";
        object? r = cmd.ExecuteScalar();
        return r is long l ? (int)l : Convert.ToInt32(r, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void MigrateToV1(Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS http_exchange (
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              captured_at TEXT NOT NULL,
              method TEXT NOT NULL,
              scheme TEXT NOT NULL,
              host TEXT NOT NULL,
              port INTEGER NOT NULL,
              path TEXT NOT NULL,
              query_text TEXT,
              url_display TEXT NOT NULL,
              request_headers_json TEXT,
              request_body_blob BLOB,
              request_body_length INTEGER NOT NULL,
              request_body_truncated INTEGER NOT NULL,
              response_status INTEGER,
              response_headers_json TEXT,
              response_body_blob BLOB,
              response_body_length INTEGER NOT NULL,
              response_body_truncated INTEGER NOT NULL,
              duration_ms INTEGER,
              error_text TEXT,
              client_process_id INTEGER,
              client_process_name TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_http_exchange_captured_at ON http_exchange(captured_at);
            CREATE INDEX IF NOT EXISTS idx_http_exchange_host ON http_exchange(host);
            CREATE INDEX IF NOT EXISTS idx_http_exchange_response_status ON http_exchange(response_status);
            """;
        cmd.ExecuteNonQuery();
    }
}
