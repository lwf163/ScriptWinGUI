using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace Swg.Capture;

/// <summary>
/// ETW 形态 A：单会话内合并音频轨与窗口生命周期轨（分流到有界队列 + 独立 worker），推送到 <see cref="NotificationPushHub"/>。
/// </summary>
public sealed class CaptureEtwProbe : IDisposable
{
    /// <summary>通知载荷中与弱语义一致的类型名。</summary>
    public const string AudioActivityEventType = "AudioActivity";

    private readonly Guid _listenWindowId;
    private readonly EtwCaptureOptions _etw;
    private readonly NotificationCaptureOptions _notification;

    private readonly object _stateLock = new();
    private readonly TaskCompletionSource<bool> _sessionReady = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly HashSet<string> _audioProviderSet;
    private readonly HashSet<string> _windowProviderSet;
    private readonly HashSet<string> _subscribedWindowEventTypes;

    private Channel<EtwRawEvent>? _audioChannel;
    private Channel<EtwRawEvent>? _windowChannel;
    private CancellationTokenSource? _workerCts;
    private Task? _audioWorkerTask;
    private Task? _windowWorkerTask;
    private Thread? _sessionThread;
    private TraceEventSession? _session;
    private volatile bool _disposed;

    private readonly Dictionary<string, DateTimeOffset> _debounceAudio = new();
    private readonly Dictionary<string, DateTimeOffset> _debounceWindow = new();
    private readonly object _debounceAudioLock = new();
    private readonly object _debounceWindowLock = new();

    public CaptureEtwProbe(Guid listenWindowId, EtwCaptureOptions etw, NotificationCaptureOptions notification)
    {
        _listenWindowId = listenWindowId;
        _etw = etw;
        _notification = notification;

        _audioProviderSet = NormalizeProviderNames(etw.ProviderNames);
        _windowProviderSet = NormalizeProviderNames(etw.WindowProviderNames);
        IReadOnlyList<string> winTypes = WindowCaptureEventTypes.NormalizeSubscription(etw.WindowEventTypes);
        _subscribedWindowEventTypes = new HashSet<string>(winTypes, StringComparer.Ordinal);
    }

    public bool IsEnabled { get; private set; }

    /// <summary>
    /// 在 <see cref="ListenWindowOptions.EnableEtwProbe"/> 为 true 时由宿主调用。
    /// 音频轨：Provider 非空即启用；窗口轨：Provider 与 <see cref="EtwCaptureOptions.WindowEventTypes"/> 均非空才启用。
    /// 二者皆无可启用内容时静默返回。
    /// </summary>
    public void StartIfConfigured()
    {
        ValidateWindowTrackOrThrow();

        List<string> mergedProviders = _audioProviderSet
            .Concat(_windowProviderSet)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (mergedProviders.Count == 0)
            return;

        bool audioOn = _audioProviderSet.Count > 0;
        bool windowOn = _windowProviderSet.Count > 0 && _subscribedWindowEventTypes.Count > 0;

        int audioCap = Math.Clamp(_etw.QueueCapacity, 64, 1_000_000);
        int windowCap = _etw.WindowQueueCapacity > 0
            ? Math.Clamp(_etw.WindowQueueCapacity, 64, 1_000_000)
            : audioCap;

        _workerCts = new CancellationTokenSource();
        CancellationToken wct = _workerCts.Token;

        if (audioOn)
        {
            _audioChannel = CreateChannel(audioCap);
            _audioWorkerTask = WorkerLoopAsync(_audioChannel.Reader, wct, processAudio: true);
        }

        if (windowOn)
        {
            _windowChannel = CreateChannel(windowCap);
            _windowWorkerTask = WorkerLoopAsync(_windowChannel.Reader, wct, processAudio: false);
        }

        _sessionThread = new Thread(() => SessionThreadProc(mergedProviders))
        {
            IsBackground = true,
            Name = "Swg.Capture.CaptureEtwProbe.Session",
        };
        _sessionThread.Start();

        try
        {
            _ = _sessionReady.Task.GetAwaiter().GetResult();
        }
        catch
        {
            Dispose();
            throw;
        }

        IsEnabled = true;
    }

    private void ValidateWindowTrackOrThrow()
    {
        bool hasWp = _windowProviderSet.Count > 0;
        bool hasWt = _subscribedWindowEventTypes.Count > 0;
        if (hasWp != hasWt)
        {
            throw new ArgumentException(
                "ETW 窗口轨要求 WindowProviderNames 与 WindowEventTypes 同时非空或同时为空。");
        }

        if (hasWt)
        {
            WindowCaptureEventTypes.ValidateSubscription(
                _subscribedWindowEventTypes.ToList(),
                nameof(EtwCaptureOptions.WindowEventTypes));
        }
    }

    private static Channel<EtwRawEvent> CreateChannel(int cap) =>
        Channel.CreateBounded<EtwRawEvent>(new BoundedChannelOptions(cap)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });

    private static HashSet<string> NormalizeProviderNames(IReadOnlyList<string>? raw)
    {
        if (raw is null || raw.Count == 0)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<string> q = raw
            .Select(static s => s.Trim())
            .Where(static s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);
        return new HashSet<string>(q, StringComparer.OrdinalIgnoreCase);
    }

    private void SessionThreadProc(IReadOnlyList<string> providers)
    {
        string sessionName = "SwgCaptureEtw_" + Guid.NewGuid().ToString("N");
        TraceEventLevel level = MapLevel(_etw.Level);
        ulong keyword = _etw.MatchAnyKeyword ?? ulong.MaxValue;

        try
        {
            var session = new TraceEventSession(sessionName, TraceEventSessionOptions.Create);
            lock (_stateLock)
            {
                if (_disposed)
                {
                    session.Dispose();
                    _sessionReady.TrySetException(new ObjectDisposedException(nameof(CaptureEtwProbe)));
                    return;
                }

                _session = session;
            }

            foreach (string p in providers)
            {
                session.EnableProvider(p, level, keyword);
            }

            session.Source.Dynamic.All += OnTraceEvent;
            _sessionReady.TrySetResult(true);
            session.Source.Process();
        }
        catch (Exception ex)
        {
            _sessionReady.TrySetException(
                new InvalidOperationException(
                    "无法启动 ETW 实时会话。请确认 Provider 名称正确；若以管理员身份运行仍失败，可关闭 EnableEtwProbe 或调整 Provider 列表。",
                    ex));
        }
        finally
        {
            TraceEventSession? toDispose;
            lock (_stateLock)
            {
                toDispose = _session;
                _session = null;
            }

            toDispose?.Dispose();
        }
    }

    private void OnTraceEvent(TraceEvent data)
    {
        if (_disposed)
            return;

        int pid = data.ProcessID;
        int tid = data.ThreadID;
        string provider = data.ProviderName ?? "";
        string eventName = data.EventName ?? "";
        var raw = new EtwRawEvent(pid, tid, provider, eventName);

        if (_audioChannel is not null && _audioProviderSet.Contains(provider))
        {
            _ = _audioChannel.Writer.TryWrite(raw);
            return;
        }

        if (_windowChannel is not null && _windowProviderSet.Contains(provider))
            _ = _windowChannel.Writer.TryWrite(raw);
    }

    private async Task WorkerLoopAsync(ChannelReader<EtwRawEvent> reader, CancellationToken ct, bool processAudio)
    {
        try
        {
            while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                while (reader.TryRead(out EtwRawEvent ev))
                {
                    if (_disposed)
                        return;

                    if (processAudio)
                        ProcessOneAudio(ev);
                    else
                        ProcessOneWindow(ev);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常关闭
        }
    }

    private void ProcessOneAudio(EtwRawEvent ev)
    {
        string? processName = TryGetProcessName(ev.ProcessId);
        if (!PassesProcessFilter(processName))
            return;

        string debounceKey = $"etw:{ev.ProcessId}:{ev.ProviderName}:{ev.EventName}";
        lock (_debounceAudioLock)
        {
            if (!ShouldDebounce(_debounceAudio, debounceKey, _notification.DebounceMs))
                return;
        }

        string extracted = string.IsNullOrEmpty(ev.EventName)
            ? ev.ProviderName
            : $"{ev.ProviderName}/{ev.EventName}";

        var payload = new NotificationEventPayload
        {
            EventType = AudioActivityEventType,
            CapturedAt = DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
            ExtractedText = extracted,
            ProcessId = ev.ProcessId <= 0 ? null : ev.ProcessId,
            ProcessName = processName,
            ThreadId = ev.ThreadId <= 0 ? null : ev.ThreadId,
            WindowHandle = 0,
        };

        string json = NotificationEventSerializer.Event(_listenWindowId, payload);
        _ = NotificationPushHub.PushJsonAsync(json);
    }

    private void ProcessOneWindow(EtwRawEvent ev)
    {
        string? processName = TryGetProcessName(ev.ProcessId);
        if (!PassesProcessFilter(processName))
            return;

        List<string> mapped = WindowEtwEventMap.TryMap(ev.ProviderName, ev.EventName, _subscribedWindowEventTypes);
        if (mapped.Count == 0)
            return;

        foreach (string eventType in mapped)
        {
            string debounceKey = $"etw_win:{ev.ProcessId}:{ev.ProviderName}:{ev.EventName}:{eventType}";
            lock (_debounceWindowLock)
            {
                if (!ShouldDebounce(_debounceWindow, debounceKey, _notification.DebounceMs))
                    continue;
            }

            string extracted = string.IsNullOrEmpty(ev.EventName)
                ? ev.ProviderName
                : $"{ev.ProviderName}/{ev.EventName}";

            var payload = new NotificationEventPayload
            {
                EventType = eventType,
                CapturedAt = DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                ExtractedText = extracted,
                ProcessId = ev.ProcessId <= 0 ? null : ev.ProcessId,
                ProcessName = processName,
                ThreadId = ev.ThreadId <= 0 ? null : ev.ThreadId,
                WindowHandle = 0,
            };

            string json = NotificationEventSerializer.Event(_listenWindowId, payload);
            _ = NotificationPushHub.PushJsonAsync(json);
        }
    }

    private static bool ShouldDebounce(Dictionary<string, DateTimeOffset> dict, string key, int debounceMs)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (dict.TryGetValue(key, out DateTimeOffset last) &&
            (now - last).TotalMilliseconds < debounceMs)
        {
            return false;
        }

        dict[key] = now;
        return true;
    }

    private bool PassesProcessFilter(string? processName)
    {
        if (_notification.ProcessNameContains is null || _notification.ProcessNameContains.Count == 0)
            return true;

        if (string.IsNullOrEmpty(processName))
            return false;

        foreach (string fragment in _notification.ProcessNameContains)
        {
            if (processName.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string? TryGetProcessName(int pid)
    {
        if (pid <= 0)
            return null;
        try
        {
            using Process p = Process.GetProcessById(pid);
            return p.ProcessName;
        }
        catch
        {
            return null;
        }
    }

    private static TraceEventLevel MapLevel(byte? level)
    {
        if (!level.HasValue)
            return TraceEventLevel.Informational;

        int v = level.Value;
        if (v < (int)TraceEventLevel.Always || v > (int)TraceEventLevel.Verbose)
            return TraceEventLevel.Informational;

        return (TraceEventLevel)v;
    }

    public void Dispose()
    {
        lock (_stateLock)
        {
            if (_disposed)
                return;
            _disposed = true;
        }

        TraceEventSession? s;
        lock (_stateLock)
        {
            s = _session;
            _session = null;
        }

        s?.Dispose();

        if (_sessionThread is not null)
        {
            _ = _sessionThread.Join(TimeSpan.FromSeconds(8));
            _sessionThread = null;
        }

        _workerCts?.Cancel();
        _audioChannel?.Writer.TryComplete();
        _windowChannel?.Writer.TryComplete();

        try
        {
            _audioWorkerTask?.Wait(TimeSpan.FromSeconds(5));
            _windowWorkerTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // 忽略 worker 异常，避免 Dispose 抛错
        }

        _workerCts?.Dispose();
        _workerCts = null;
        _audioChannel = null;
        _windowChannel = null;
        _audioWorkerTask = null;
        _windowWorkerTask = null;
    }

    private readonly struct EtwRawEvent
    {
        public EtwRawEvent(int processId, int threadId, string providerName, string eventName)
        {
            ProcessId = processId;
            ThreadId = threadId;
            ProviderName = providerName;
            EventName = eventName;
        }

        public int ProcessId { get; }
        public int ThreadId { get; }
        public string ProviderName { get; }
        public string EventName { get; }
    }
}
