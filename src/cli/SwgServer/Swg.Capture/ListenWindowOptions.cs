namespace Swg.Capture;

/// <summary>
/// 单个监听窗口（逻辑会话）的运行参数。
/// </summary>
public sealed class ListenWindowOptions
{
    /// <summary>存放 <c>.sqlite</c> 的目录；不存在时会创建。</summary>
    public string StorageDirectory { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Swg", "capture");

    /// <summary>本地代理监听端口；0 表示由操作系统分配。</summary>
    public int ProxyListenPort { get; set; }

    /// <summary>请求/响应 body 单段最大入库字节（超出则截断并标记）。</summary>
    public int MaxBodyBytesPerPart { get; set; } = 256 * 1024;

    /// <summary>定时刷盘间隔（毫秒）。</summary>
    public int FlushIntervalMs { get; set; } = 2000;

    /// <summary>单次批量 INSERT 最大行数。</summary>
    public int FlushBatchMaxRows { get; set; } = 200;

    /// <summary>单次批量 INSERT 最大估算字节（与行数组合触发）。</summary>
    public long FlushBatchMaxBytes { get; set; } = 4 * 1024 * 1024;

    public HttpCaptureFilterRules TrafficFilter { get; set; } = new();

    public MitmCertificateOptions Mitm { get; set; } = new();

    public bool EnableNotifications { get; set; } = true;

    public NotificationCaptureOptions Notification { get; set; } = new();

    public EtwCaptureOptions Etw { get; set; } = new();

    public bool EnableEtwProbe { get; set; }

    /// <summary>流量周期统计推送间隔（毫秒）。</summary>
    public int TrafficStatsIntervalMs { get; set; } = 10_000;
}
