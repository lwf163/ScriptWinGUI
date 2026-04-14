namespace Swg.Capture;

/// <summary>
/// ETW（Event Tracing for Windows）弱语义「有声音活动」订阅参数；Provider 未配置时不启动会话。
/// </summary>
public sealed class EtwCaptureOptions
{
    /// <summary>
    /// 要启用的 ETW Provider 名称列表（字符串，如 Microsoft-Windows-Audio）；
    /// 为 null 或空则<strong>不启动</strong> ETW（即使 <see cref="ListenWindowOptions.EnableEtwProbe"/> 为 true）。
    /// </summary>
    public IReadOnlyList<string>? ProviderNames { get; set; }

    /// <summary>传给 EnableProvider 的 matchAnyKeyword；未指定时为 ulong.MaxValue。</summary>
    public ulong? MatchAnyKeyword { get; set; }

    /// <summary>TraceEventLevel 数值（0–5）；未指定时默认为 Informational。</summary>
    public byte? Level { get; set; }

    /// <summary>回调到 worker 之间的有界队列容量（条数）；队列满时按 DropOldest 丢弃最旧项。</summary>
    public int QueueCapacity { get; set; } = 4096;

    /// <summary>
    /// 窗口生命周期 ETW 轨的 Provider 列表；与 <see cref="WindowEventTypes"/> 需同时非空才会启用该轨。
    /// </summary>
    public IReadOnlyList<string>? WindowProviderNames { get; set; }

    /// <summary>窗口轨订阅的 <see cref="WindowCaptureEventTypes"/> 子集。</summary>
    public IReadOnlyList<string>? WindowEventTypes { get; set; }

    /// <summary>窗口轨有界队列容量；≤0 时使用 <see cref="QueueCapacity"/>。</summary>
    public int WindowQueueCapacity { get; set; }
}
