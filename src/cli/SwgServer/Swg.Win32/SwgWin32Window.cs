namespace Swg.Win32;

/// <summary>
/// 窗口相关静态业务函数。
/// </summary>
public static class SwgWin32Window
{
    public static string FindWindowHandle(
        string? titleContains,
        string? classNameEquals,
        uint? processId,
        bool visibleOnly)
    {
        nint firstMatched = 0;

        Win32Native.EnumWindowsCollect(hWnd =>
        {
            if (visibleOnly && !Win32Native.IsWindowVisible(hWnd))
                return false;

            if (processId.HasValue)
            {
                uint pid = Win32Native.GetProcessId(hWnd);
                if (pid != processId.Value)
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(classNameEquals))
            {
                string cls = Win32Native.TryGetClassName(hWnd);
                if (!string.Equals(cls, classNameEquals.Trim(), StringComparison.Ordinal))
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(titleContains))
            {
                string title = Win32Native.TryGetWindowTitleOrEmpty(hWnd);
                if (title.IndexOf(titleContains.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            return true;
        }, out firstMatched);

        if (firstMatched == 0)
            throw new InvalidOperationException("Window not found.");

        if (!Win32Native.TryGetWindowRect(firstMatched, out _))
            throw new InvalidOperationException("Window rect unavailable.");

        return Win32Native.FormatHandle(firstMatched);
    }

    public static string GetForegroundWindowHandle()
    {
        nint hwnd = Win32Native.GetForegroundWindow();
        if (hwnd == 0)
            throw new InvalidOperationException("Foreground window not found.");
        return Win32Native.FormatHandle(hwnd);
    }

    public static void SetForegroundWindow(string? windowHandle)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(windowHandle, nameof(windowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Window handle invalid.");

        if (!Win32Native.SetForegroundWindow(hwnd))
            throw new InvalidOperationException("SetForegroundWindow failed.");
    }

    public static WindowInfo GetWindowInfo(string? windowHandle)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(windowHandle, nameof(windowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Window handle invalid.");

        string title = Win32Native.TryGetWindowTitleOrEmpty(hwnd);
        string className = Win32Native.TryGetClassName(hwnd);
        uint pid = Win32Native.GetProcessId(hwnd);

        if (!Win32Native.TryGetWindowRect(hwnd, out var rect))
            throw new InvalidOperationException("GetWindowRect failed.");

        var rd = Win32Native.ToRectDto(rect);
        return new WindowInfo(
            Win32Native.FormatHandle(hwnd),
            title,
            className,
            pid,
            new WindowRect(rd.Left, rd.Top, rd.Right, rd.Bottom, rd.Width, rd.Height));
    }

    public static void SetWindowPositionResize(
        string? windowHandle,
        int left,
        int top,
        int width,
        int height)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(windowHandle, nameof(windowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Window handle invalid.");

        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width/Height 必须为正。");

        // 不改变 z-order，不强制激活；并要求显示窗口。
        uint flags = Win32Native.SwpShowWindow | Win32Native.SwpNoZorder | Win32Native.SwpNoActivate;
        bool ok = Win32Native.SetWindowPos(hwnd, 0, left, top, width, height, flags);
        if (!ok)
            throw new InvalidOperationException("SetWindowPos failed.");
    }

    public static void SetWindowState(string? windowHandle, string? state)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(windowHandle, nameof(windowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Window handle invalid.");

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State 必填。", nameof(state));

        string s = state.Trim();
        int cmdShow = s.ToLowerInvariant() switch
        {
            "minimized" or "min" or "minimize" => Win32Native.SwMinimize,
            "maximized" or "max" or "maximize" => Win32Native.SwMaximize,
            "restored" or "restore" or "normal" => Win32Native.SwRestore,
            _ => throw new ArgumentException("State 无效（Minimized/Maximized/Restored）。", nameof(state)),
        };

        int r = Win32Native.ShowWindow(hwnd, cmdShow);
        // ShowWindow 的返回值语义较复杂；这里仅在返回值明确为 0 且窗口不可见时判失败。
        if (r == 0 && !Win32Native.IsWindowVisible(hwnd))
            throw new InvalidOperationException("ShowWindow failed.");
    }

    public static void CloseWindow(string? windowHandle)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(windowHandle, nameof(windowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Window handle invalid.");

        // WM_CLOSE 通常由目标窗口自行处理并触发退出流程。
        bool ok = Win32Native.PostMessage(hwnd, Win32Native.WmClose, 0, 0);
        if (!ok)
            throw new InvalidOperationException("PostMessage(WM_CLOSE) failed.");
    }

    public static uint GetWindowProcessId(string? windowHandle)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(windowHandle, nameof(windowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Window handle invalid.");

        uint pid = Win32Native.GetProcessId(hwnd);
        if (pid == 0)
            throw new InvalidOperationException("ProcessId not found for window.");

        return pid;
    }

    public static IReadOnlyList<string> EnumChildWindowHandles(string? parentWindowHandle)
    {
        nint parentHwnd = Win32Native.ParseHandleOrThrow(parentWindowHandle, nameof(parentWindowHandle));
        if (!Win32Native.IsWindow(parentHwnd))
            throw new InvalidOperationException("Parent window handle invalid.");

        Win32Native.EnumChildWindowsCollect(parentHwnd, _ => true, out List<nint> matches);
        return matches.Select(static x => Win32Native.FormatHandle(x)).ToArray();
    }

    public static string FindChildWindowHandle(
        string? parentWindowHandle,
        string? titleContains,
        string? classNameEquals,
        bool visibleOnly)
    {
        nint parentHwnd = Win32Native.ParseHandleOrThrow(parentWindowHandle, nameof(parentWindowHandle));
        if (!Win32Native.IsWindow(parentHwnd))
            throw new InvalidOperationException("Parent window handle invalid.");

        Win32Native.EnumChildWindowsCollect(
            parentHwnd,
            hWnd =>
            {
                if (visibleOnly && !Win32Native.IsWindowVisible(hWnd))
                    return false;

                if (!string.IsNullOrWhiteSpace(classNameEquals))
                {
                    string cls = Win32Native.TryGetClassName(hWnd);
                    if (!string.Equals(cls, classNameEquals.Trim(), StringComparison.Ordinal))
                        return false;
                }

                if (!string.IsNullOrWhiteSpace(titleContains))
                {
                    string title = Win32Native.TryGetWindowTitleOrEmpty(hWnd);
                    if (title.IndexOf(titleContains.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
                        return false;
                }

                return true;
            },
            out List<nint> found);

        if (found.Count == 0)
            throw new InvalidOperationException("Child window not found.");

        if (!Win32Native.TryGetWindowRect(found[0], out _))
            throw new InvalidOperationException("Child window rect unavailable.");

        return Win32Native.FormatHandle(found[0]);
    }

    public static string GetWindowHandleAtPoint(int x, int y)
    {
        var p = new Win32Native.Point { X = x, Y = y };
        nint hwnd = Win32Native.WindowFromPoint(p);
        if (hwnd == 0)
            throw new InvalidOperationException("No window at point.");
        return Win32Native.FormatHandle(hwnd);
    }
}

