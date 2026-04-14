using System.Threading;

namespace Swg.Capture;

/// <summary>
/// 单个监听窗口（逻辑会话）：独占 SQLite、内存缓冲、代理与可选通知管道。
/// </summary>
public sealed class ListenWindow : IDisposable
{
    private readonly ListenWindowOptions _options;
    private readonly MemoryExchangeBuffer _buffer;
    private readonly SqliteBatchWriter _writer;
    private readonly HttpProxyPipeline _proxy;
    private readonly NotificationPipeline? _notification;
    private readonly CaptureEtwProbe? _etw;
    private readonly Timer? _flushTimer;
    private readonly Timer? _statsTimer;
    private readonly object _flushLock = new();
    private long _totalFlushedRows;
    private bool _disposed;

    public ListenWindow(Guid id, ListenWindowOptions options, string sqlitePath)
    {
        ValidateOptions(options);

        Id = id;
        _options = options;
        SqlitePath = sqlitePath;

        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

        _buffer = new MemoryExchangeBuffer();
        _writer = new SqliteBatchWriter(sqlitePath);

        _proxy = new HttpProxyPipeline(id, options, _buffer);
        _proxy.Start();

        if (options.EnableNotifications)
        {
            IReadOnlyList<string> hookSub = ResolveHookSubscription(options.Notification);
            if (hookSub.Count > 0)
            {
                _notification = new NotificationPipeline(id, options.Notification, hookSub);
                _notification.Start();
            }
        }

        if (options.EnableEtwProbe)
        {
            _etw = new CaptureEtwProbe(id, options.Etw, options.Notification);
            _etw.StartIfConfigured();
        }

        _flushTimer = new Timer(_ => FlushDue(), null, options.FlushIntervalMs, options.FlushIntervalMs);
        _statsTimer = new Timer(_ => PushStats(), null, options.TrafficStatsIntervalMs, options.TrafficStatsIntervalMs);
    }

    private static void ValidateOptions(ListenWindowOptions o)
    {
        if (o.EnableNotifications && o.Notification.HookWindowEventTypes is not null)
        {
            IReadOnlyList<string> n = WindowCaptureEventTypes.NormalizeSubscription(o.Notification.HookWindowEventTypes);
            if (n.Count > 0)
            {
                WindowCaptureEventTypes.ValidateSubscription(
                    n.ToList(),
                    nameof(NotificationCaptureOptions.HookWindowEventTypes));
            }
        }

        if (!o.EnableEtwProbe)
            return;

        bool hasWp = HasNonEmptyStrings(o.Etw.WindowProviderNames);
        bool hasWt = o.Etw.WindowEventTypes is not null &&
                     WindowCaptureEventTypes.NormalizeSubscription(o.Etw.WindowEventTypes).Count > 0;
        if (hasWp != hasWt)
        {
            throw new ArgumentException(
                "EnableEtwProbe 时，Etw.WindowProviderNames 与 Etw.WindowEventTypes 必须同时非空或同时为空（音频轨仅 Provider 时可单独存在）。");
        }

        if (hasWt)
        {
            IReadOnlyList<string> wt = WindowCaptureEventTypes.NormalizeSubscription(o.Etw.WindowEventTypes);
            WindowCaptureEventTypes.ValidateSubscription(
                wt.ToList(),
                nameof(EtwCaptureOptions.WindowEventTypes));
        }
    }

    private static bool HasNonEmptyStrings(IReadOnlyList<string>? list) =>
        list is not null && list.Any(static s => !string.IsNullOrWhiteSpace(s));

    /// <summary>
    /// <c>null</c> → 默认订阅；空列表 → 不注册 Hook；非空 → 原样（已校验）。
    /// </summary>
    private static IReadOnlyList<string> ResolveHookSubscription(NotificationCaptureOptions notification)
    {
        if (notification.HookWindowEventTypes is null)
            return WindowCaptureEventTypes.DefaultHookSubscription;

        IReadOnlyList<string> n = WindowCaptureEventTypes.NormalizeSubscription(notification.HookWindowEventTypes);
        return n;
    }

    public Guid Id { get; }

    public string SqlitePath { get; }

    public int ProxyListenPort => _proxy.ListeningPort;

    public IReadOnlyList<HttpExchangeRecord> SnapshotPendingMemory() => _buffer.SnapshotPending();

    public IReadOnlyList<HttpExchangeRecord> QueryPersistedPage(DateTimeOffset? beforeUtc, int limit, int offset) =>
        _writer.QueryPage(beforeUtc, limit, offset);

    /// <summary>合并内存未刷盘与 SQLite，按时间倒序分页（内存与库无重叠）。</summary>
    public IReadOnlyList<HttpExchangeRecord> QueryHistoryMerged(int limit, int offset, DateTimeOffset? beforeUtc)
    {
        if (limit <= 0)
            limit = 50;
        if (offset < 0)
            offset = 0;

        IReadOnlyList<HttpExchangeRecord> mem = SnapshotPendingMemory();
        int fetchCap = Math.Clamp(limit + offset + mem.Count + 64, 64, 5000);
        IReadOnlyList<HttpExchangeRecord> db = _writer.QueryPage(beforeUtc, fetchCap, 0);

        IEnumerable<HttpExchangeRecord> q = mem.Concat(db);
        if (beforeUtc.HasValue)
            q = q.Where(x => x.CapturedAt < beforeUtc.Value);

        return q
            .OrderByDescending(static x => x.CapturedAt)
            .Skip(offset)
            .Take(limit)
            .ToList();
    }

    private void FlushDue()
    {
        lock (_flushLock)
        {
            IReadOnlyList<HttpExchangeRecord> batch = _buffer.DrainAndClear();
            if (batch.Count == 0)
                return;

            _writer.InsertBatch(batch);
            Interlocked.Add(ref _totalFlushedRows, batch.Count);
        }
    }

    private void PushStats()
    {
        var stats = new TrafficSnapshotStats
        {
            PendingBufferCount = _buffer.PendingCount,
            TotalFlushedRows = Interlocked.Read(ref _totalFlushedRows),
            WindowUtc = DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
        };

        _ = TrafficPushHub.PushJsonAsync(TrafficEventSerializer.Stats(Id, stats));
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        _flushTimer?.Dispose();
        _statsTimer?.Dispose();

        _notification?.Dispose();
        _etw?.Dispose();
        _proxy.Dispose();

        FlushDue();
        _writer.Dispose();
    }
}
