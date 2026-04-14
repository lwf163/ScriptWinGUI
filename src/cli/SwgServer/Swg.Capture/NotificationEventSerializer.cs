using System.Text.Json;

namespace Swg.Capture;

internal static class NotificationEventSerializer
{
    public static string Event(Guid listenWindowId, NotificationEventPayload payload)
    {
        var envelope = new NotificationEnvelope
        {
            Type = "notification.event",
            ListenWindowId = listenWindowId,
            Payload = payload,
        };
        return JsonSerializer.Serialize(envelope, CaptureJson.Options);
    }
}

public sealed class NotificationEnvelope
{
    public string Type { get; set; } = "";
    public Guid ListenWindowId { get; set; }
    public NotificationEventPayload? Payload { get; set; }
}

public sealed class NotificationEventPayload
{
    public string EventType { get; set; } = "";
    public string CapturedAt { get; set; } = "";
    public string? ExtractedText { get; set; }
    public int? ProcessId { get; set; }
    public string? ProcessName { get; set; }

    /// <summary>ETW 等路径若可得则填充；Hook 路径通常不填。</summary>
    public int? ThreadId { get; set; }

    public nint WindowHandle { get; set; }
}
