using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;
using WinEventHook;

namespace Swg.Capture;

/// <summary>
/// WinEventHook：按 Hook 订阅注册连续区间内的 WinEvent，映射为 <see cref="WindowCaptureEventTypes"/> 后推送。
/// </summary>
public sealed class NotificationPipeline : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext(typeof(NotificationPipeline));

    private readonly Guid _listenWindowId;
    private readonly NotificationCaptureOptions _options;
    private readonly HashSet<string> _hookSubscription;
    private Thread? _uiThread;
    private uint _messageThreadId;
    private readonly List<WindowEventHook> _hooks = new();
    private readonly Dictionary<string, DateTimeOffset> _debounce = new();
    private readonly object _debounceLock = new();
    private bool _disposed;

    /// <param name="hookSubscription">已规范化且非空的订阅列表。</param>
    public NotificationPipeline(
        Guid listenWindowId,
        NotificationCaptureOptions options,
        IReadOnlyList<string> hookSubscription)
    {
        _listenWindowId = listenWindowId;
        _options = options;
        _hookSubscription = new HashSet<string>(hookSubscription, StringComparer.Ordinal);
    }

    public void Start()
    {
        if (_uiThread is not null)
            return;

        _uiThread = new Thread(UiThreadProc)
        {
            IsBackground = true,
            Name = "Swg.Capture.NotificationPipeline",
        };
        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.Start();
    }

    private void UiThreadProc()
    {
        _messageThreadId = NativeMessageLoop.GetCurrentThreadId();

        try
        {
            HashSet<WindowEvent> winEvents = WinEventWindowCaptureMap.CollectWinEvents(_hookSubscription);
            List<(WindowEvent Min, WindowEvent Max)> ranges = WinEventWindowCaptureMap.ToRanges(winEvents);

            foreach ((WindowEvent min, WindowEvent max) in ranges)
            {
                var hook = new WindowEventHook(min, max);
                hook.EventReceived += OnWinEvent;
                _hooks.Add(hook);
                if (!hook.TryHookGlobal())
                    throw new InvalidOperationException("WinEventHook.TryHookGlobal 失败（可能需要权限）。");
            }

            while (NativeMessageLoop.GetMessage(out NativeMessageLoop.MSG msg, 0, 0, 0) > 0)
            {
                _ = NativeMessageLoop.TranslateMessage(ref msg);
                _ = NativeMessageLoop.DispatchMessage(ref msg);
            }
        }
        finally
        {
            foreach (WindowEventHook h in _hooks)
            {
                _ = h.TryUnhook();
                h.Dispose();
            }

            _hooks.Clear();
        }
    }

    private void OnWinEvent(object? sender, WinEventHookEventArgs e)
    {
        if (_disposed)
            return;

        try
        {
            nint hwnd = e.WindowHandle;
            if (hwnd == nint.Zero)
                return;

            string title = GetWindowTitle(hwnd);
            if (!string.IsNullOrEmpty(_options.TitleContains) &&
                !string.IsNullOrEmpty(title) &&
                !title.Contains(_options.TitleContains, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            uint pid = GetWindowPid(hwnd);
            string? processName = TryGetProcessName((int)pid);
            if (!PassesProcessFilter(processName))
                return;

            foreach (string payloadType in WinEventWindowCaptureMap.Expand(e.EventType, _hookSubscription))
            {
                string debounceKey = $"wev:{pid}:{hwnd}:{e.EventType}:{payloadType}";
                lock (_debounceLock)
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    if (_debounce.TryGetValue(debounceKey, out DateTimeOffset last) &&
                        (now - last).TotalMilliseconds < _options.DebounceMs)
                    {
                        continue;
                    }

                    _debounce[debounceKey] = now;
                }

                string extracted = string.IsNullOrWhiteSpace(title) ? payloadType : title;
                var payload = new NotificationEventPayload
                {
                    EventType = payloadType,
                    CapturedAt = DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                    ExtractedText = extracted,
                    ProcessId = pid == 0 ? null : (int)pid,
                    ProcessName = processName,
                    ThreadId = null,
                    WindowHandle = hwnd,
                };

                string json = NotificationEventSerializer.Event(_listenWindowId, payload);
                _ = NotificationPushHub.PushJsonAsync(json);
            }
        }
        catch (Exception ex)
        {
            // 通知路径不阻断消息泵
            Logger.Warning(ex, "WinEvent 通知处理异常（已吞掉以保持消息泵）");
        }
    }

    private bool PassesProcessFilter(string? processName)
    {
        if (_options.ProcessNameContains is null || _options.ProcessNameContains.Count == 0)
            return true;

        if (string.IsNullOrEmpty(processName))
            return false;

        foreach (string fragment in _options.ProcessNameContains)
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

    private static string GetWindowTitle(nint hwnd)
    {
        if (hwnd == nint.Zero)
            return "";

        int len = GetWindowTextLength(hwnd);
        if (len <= 0)
            return "";

        var sb = new StringBuilder(len + 1);
        _ = GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static uint GetWindowPid(nint hwnd)
    {
        _ = GetWindowThreadProcessId(hwnd, out uint pid);
        return pid;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        for (int i = 0; i < 200 && _messageThreadId == 0; i++)
            Thread.Sleep(10);

        if (_messageThreadId != 0)
        {
            _ = NativeMessageLoop.PostThreadMessage(_messageThreadId, NativeMessageLoop.WM_QUIT, 0, 0);
        }

        if (_uiThread is not null)
        {
            if (!_uiThread.Join(TimeSpan.FromSeconds(5)))
            {
                // 线程未在时限内结束则放弃等待，避免死锁宿主
            }

            _uiThread = null;
        }
    }
}
