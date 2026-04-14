using System.Text.Json;

namespace Swg.Capture;

internal static class TrafficEventSerializer
{
    public static string ExchangeSummary(Guid listenWindowId, HttpExchangeRecord row)
    {
        var payload = new TrafficExchangeSummaryPayload
        {
            Method = row.Method,
            UrlDisplay = row.UrlDisplay,
            Host = row.Host,
            ResponseStatus = row.ResponseStatus,
            DurationMs = row.DurationMs,
            ErrorText = row.ErrorText,
            CapturedAt = row.CapturedAt.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
        };

        var envelope = new TrafficEnvelope<TrafficExchangeSummaryPayload>
        {
            Type = "traffic.exchange",
            ListenWindowId = listenWindowId,
            Payload = payload,
        };

        return JsonSerializer.Serialize(envelope, CaptureJson.Options);
    }

    public static string Stats(Guid listenWindowId, TrafficSnapshotStats stats)
    {
        var envelope = new TrafficEnvelope<TrafficSnapshotStats>
        {
            Type = "traffic.stats",
            ListenWindowId = listenWindowId,
            Payload = stats,
        };
        return JsonSerializer.Serialize(envelope, CaptureJson.Options);
    }
}

internal sealed class TrafficEnvelope<T>
{
    public string Type { get; set; } = "";
    public Guid ListenWindowId { get; set; }
    public T? Payload { get; set; }
}

internal sealed class TrafficExchangeSummaryPayload
{
    public string Method { get; set; } = "";
    public string UrlDisplay { get; set; } = "";
    public string Host { get; set; } = "";
    public int? ResponseStatus { get; set; }
    public int? DurationMs { get; set; }
    public string? ErrorText { get; set; }
    public string CapturedAt { get; set; } = "";
}

public sealed class TrafficSnapshotStats
{
    public int PendingBufferCount { get; set; }
    public long TotalFlushedRows { get; set; }
    public string? WindowUtc { get; set; }
}
