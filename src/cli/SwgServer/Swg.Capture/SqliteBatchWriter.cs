using System.Data;
using Microsoft.Data.Sqlite;

namespace Swg.Capture;

/// <summary>
/// 单监听窗口独占的 SQLite 写入器：事务批量 INSERT。
/// </summary>
public sealed class SqliteBatchWriter : IDisposable
{
    private readonly SqliteConnection _connection;
    private bool _disposed;

    public SqliteBatchWriter(string databaseFilePath)
    {
        string dir = Path.GetDirectoryName(databaseFilePath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databaseFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();

        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        SqliteSchema.Apply(_connection);
    }

    public void InsertBatch(IReadOnlyList<HttpExchangeRecord> rows)
    {
        if (rows.Count == 0)
            return;

        using SqliteTransaction tx = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
        using SqliteCommand cmd = _connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO http_exchange (
              captured_at, method, scheme, host, port, path, query_text, url_display,
              request_headers_json, request_body_blob, request_body_length, request_body_truncated,
              response_status, response_headers_json, response_body_blob, response_body_length, response_body_truncated,
              duration_ms, error_text, client_process_id, client_process_name
            ) VALUES (
              $captured_at, $method, $scheme, $host, $port, $path, $query_text, $url_display,
              $request_headers_json, $request_body_blob, $request_body_length, $request_body_truncated,
              $response_status, $response_headers_json, $response_body_blob, $response_body_length, $response_body_truncated,
              $duration_ms, $error_text, $client_process_id, $client_process_name
            );
            """;

        foreach (HttpExchangeRecord r in rows)
        {
            cmd.Parameters.Clear();
            AddParams(cmd, r);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
    }

    public IReadOnlyList<HttpExchangeRecord> QueryPage(
        DateTimeOffset? beforeUtc,
        int limit,
        int offset)
    {
        if (limit <= 0)
            limit = 50;
        if (offset < 0)
            offset = 0;

        using var cmd = _connection.CreateCommand();
        if (beforeUtc.HasValue)
        {
            cmd.CommandText = """
                SELECT id, captured_at, method, scheme, host, port, path, query_text, url_display,
                  request_headers_json, request_body_blob, request_body_length, request_body_truncated,
                  response_status, response_headers_json, response_body_blob, response_body_length, response_body_truncated,
                  duration_ms, error_text, client_process_id, client_process_name
                FROM http_exchange
                WHERE captured_at < $before
                ORDER BY captured_at DESC
                LIMIT $limit OFFSET $offset;
                """;
            cmd.Parameters.AddWithValue("$before", beforeUtc.Value.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        }
        else
        {
            cmd.CommandText = """
                SELECT id, captured_at, method, scheme, host, port, path, query_text, url_display,
                  request_headers_json, request_body_blob, request_body_length, request_body_truncated,
                  response_status, response_headers_json, response_body_blob, response_body_length, response_body_truncated,
                  duration_ms, error_text, client_process_id, client_process_name
                FROM http_exchange
                ORDER BY captured_at DESC
                LIMIT $limit OFFSET $offset;
                """;
        }

        cmd.Parameters.AddWithValue("$limit", limit);
        cmd.Parameters.AddWithValue("$offset", offset);

        var list = new List<HttpExchangeRecord>();
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(ReadRow(reader));
        }

        return list;
    }

    private static HttpExchangeRecord ReadRow(SqliteDataReader reader)
    {
        return new HttpExchangeRecord
        {
            Id = reader.GetInt64(0),
            CapturedAt = DateTimeOffset.Parse(reader.GetString(1), System.Globalization.CultureInfo.InvariantCulture),
            Method = reader.GetString(2),
            Scheme = reader.GetString(3),
            Host = reader.GetString(4),
            Port = reader.GetInt32(5),
            Path = reader.GetString(6),
            QueryText = reader.IsDBNull(7) ? null : reader.GetString(7),
            UrlDisplay = reader.GetString(8),
            RequestHeadersJson = reader.IsDBNull(9) ? null : reader.GetString(9),
            RequestBodyBlob = reader.IsDBNull(10) ? null : (byte[])reader[10],
            RequestBodyLength = reader.GetInt32(11),
            RequestBodyTruncated = reader.GetInt32(12),
            ResponseStatus = reader.IsDBNull(13) ? null : reader.GetInt32(13),
            ResponseHeadersJson = reader.IsDBNull(14) ? null : reader.GetString(14),
            ResponseBodyBlob = reader.IsDBNull(15) ? null : (byte[])reader[15],
            ResponseBodyLength = reader.GetInt32(16),
            ResponseBodyTruncated = reader.GetInt32(17),
            DurationMs = reader.IsDBNull(18) ? null : reader.GetInt32(18),
            ErrorText = reader.IsDBNull(19) ? null : reader.GetString(19),
            ClientProcessId = reader.IsDBNull(20) ? null : reader.GetInt32(20),
            ClientProcessName = reader.IsDBNull(21) ? null : reader.GetString(21),
        };
    }

    private static void AddParams(SqliteCommand cmd, HttpExchangeRecord r)
    {
        cmd.Parameters.AddWithValue("$captured_at", r.CapturedAt.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$method", r.Method);
        cmd.Parameters.AddWithValue("$scheme", r.Scheme);
        cmd.Parameters.AddWithValue("$host", r.Host);
        cmd.Parameters.AddWithValue("$port", r.Port);
        cmd.Parameters.AddWithValue("$path", r.Path);
        cmd.Parameters.AddWithValue("$query_text", (object?)r.QueryText ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$url_display", r.UrlDisplay);
        cmd.Parameters.AddWithValue("$request_headers_json", (object?)r.RequestHeadersJson ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$request_body_blob", (object?)r.RequestBodyBlob ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$request_body_length", r.RequestBodyLength);
        cmd.Parameters.AddWithValue("$request_body_truncated", r.RequestBodyTruncated);
        cmd.Parameters.AddWithValue("$response_status", (object?)r.ResponseStatus ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$response_headers_json", (object?)r.ResponseHeadersJson ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$response_body_blob", (object?)r.ResponseBodyBlob ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$response_body_length", r.ResponseBodyLength);
        cmd.Parameters.AddWithValue("$response_body_truncated", r.ResponseBodyTruncated);
        cmd.Parameters.AddWithValue("$duration_ms", (object?)r.DurationMs ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$error_text", (object?)r.ErrorText ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$client_process_id", (object?)r.ClientProcessId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$client_process_name", (object?)r.ClientProcessName ?? DBNull.Value);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _connection.Dispose();
    }
}
