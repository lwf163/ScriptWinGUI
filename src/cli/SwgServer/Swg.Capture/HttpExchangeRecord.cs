namespace Swg.Capture;

/// <summary>
/// 一条 HTTP 交换（请求—响应）在内存与 SQLite 中的领域模型，对应表 <c>http_exchange</c>。
/// </summary>
public sealed class HttpExchangeRecord
{
    /// <summary>自增主键；未落库时为 0。</summary>
    public long Id { get; set; }

    public DateTimeOffset CapturedAt { get; set; }

    public string Method { get; set; } = "";

    public string Scheme { get; set; } = "http";

    public string Host { get; set; } = "";

    public int Port { get; set; }

    public string Path { get; set; } = "";

    public string? QueryText { get; set; }

    public string UrlDisplay { get; set; } = "";

    public string? RequestHeadersJson { get; set; }

    public byte[]? RequestBodyBlob { get; set; }

    public int RequestBodyLength { get; set; }

    public int RequestBodyTruncated { get; set; }

    public int? ResponseStatus { get; set; }

    public string? ResponseHeadersJson { get; set; }

    public byte[]? ResponseBodyBlob { get; set; }

    public int ResponseBodyLength { get; set; }

    public int ResponseBodyTruncated { get; set; }

    public int? DurationMs { get; set; }

    public string? ErrorText { get; set; }

    public int? ClientProcessId { get; set; }

    public string? ClientProcessName { get; set; }
}
