using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Swg.Win32;

/// <summary>
/// Win32 P/Invoke 封装（仅供本模块内部使用）。
/// </summary>
internal static class Win32Native
{
    internal const int SmXVirtualScreen = 76;
    internal const int SmYVirtualScreen = 77;
    internal const int SmCxVirtualScreen = 78;
    internal const int SmCyVirtualScreen = 79;

    internal const int SmCxScreen = 0;
    internal const int SmCyScreen = 1;

    internal const int LogPixelsX = 88;
    internal const int LogPixelsY = 90;

    internal const int WmClose = 0x0010;
    internal const int WmCommand = 0x0111;

    internal const int WmKeyDown = 0x0100;
    internal const int WmKeyUp = 0x0101;
    internal const int WmChar = 0x0102;

    internal const uint CfuUnicodeText = 13; // CF_UNICODETEXT

    internal const uint GmemMoveable = 0x0002;

    internal const uint SwpNoZorder = 0x0004;
    internal const uint SwpNoActivate = 0x0010;
    internal const uint SwpShowWindow = 0x0040;

    internal const int SwMinimize = 6;
    internal const int SwMaximize = 3;
    internal const int SwRestore = 9;

    internal const uint VkControl = 0x11;
    internal const uint VkMenu = 0x12; // Alt
    internal const uint VkShift = 0x10;
    internal const uint VkLWin = 0x5B;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        internal int X;
        internal int Y;
    }

    internal delegate bool EnumWindowsProc(nint hWnd, nint lParam);
    internal delegate bool EnumChildProc(nint hWnd, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumChildWindows(nint hWndParent, EnumChildProc lpEnumFunc, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool IsWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(nint hWnd, out Rect lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern int ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint WindowFromPoint(Point point);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetDpiForSystem();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint GetDC(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern int ReleaseDC(nint hWnd, nint hDC);

    [DllImport("gdi32.dll", SetLastError = true)]
    internal static extern int GetDeviceCaps(nint hdc, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

    #region Clipboard
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool OpenClipboard(nint hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint GetClipboardData(uint uFormat);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint SetClipboardData(uint uFormat, nint hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern nint GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern nint GlobalLock(nint hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool GlobalUnlock(nint hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern nint GlobalFree(nint hMem);
    #endregion

    internal static string FormatHandle(nint hwnd)
    {
        // 以 decimal 字符串输出，避免 JSON 精度问题。
        return ((nuint)hwnd).ToString(CultureInfo.InvariantCulture);
    }

    internal static nint ParseHandleOrThrow(string? raw, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException($"{fieldName} 必填。", fieldName);

        string s = raw.Trim();
        try
        {
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                ulong v = Convert.ToUInt64(s, 16);
                return unchecked((nint)v);
            }

            if (s.StartsWith("-", StringComparison.Ordinal))
            {
                long v = long.Parse(s, CultureInfo.InvariantCulture);
                return (nint)v;
            }

            ulong u = ulong.Parse(s, CultureInfo.InvariantCulture);
            return unchecked((nint)u);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            throw new ArgumentException($"{fieldName} 格式无效（期望 decimal 或 0x...）。", fieldName);
        }
    }

    internal static nint ParseNintOrThrow(string? raw, string fieldName)
    {
        // 目前同 HWND 解析规则；后续如需严格区分可再扩展。
        return ParseHandleOrThrow(raw, fieldName);
    }

    internal static void EnumWindowsCollect(Func<nint, bool> handlePredicate, out nint firstMatched)
    {
        nint matched = 0;
        _ = EnumWindows((hWnd, _) =>
        {
            if (handlePredicate(hWnd))
            {
                matched = hWnd;
                return false;
            }
            return true;
        }, 0);

        firstMatched = matched;
    }

    internal static void EnumChildWindowsCollect(nint parentHwnd, Func<nint, bool> childPredicate, out List<nint> matches)
    {
        if (parentHwnd == 0 || !IsWindow(parentHwnd))
            throw new ArgumentException("parentHwnd 无效。", nameof(parentHwnd));

        var list = new List<nint>();
        _ = EnumChildWindows(parentHwnd, (hWnd, _) =>
        {
            if (childPredicate(hWnd))
            {
                list.Add(hWnd);
            }
            return true;
        }, 0);

        matches = list;
    }

    internal static bool TryGetWindowRect(nint hwnd, out Rect rect)
    {
        rect = default;
        if (hwnd == 0 || !IsWindow(hwnd))
            return false;
        return GetWindowRect(hwnd, out rect);
    }

    internal static string TryGetWindowText(nint hwnd)
    {
        if (hwnd == 0 || !IsWindow(hwnd))
            return string.Empty;

        int len = GetWindowTextLength(hwnd);
        if (len <= 0)
            return string.Empty;

        var sb = new StringBuilder(len + 1);
        _ = GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    internal static string TryGetClassName(nint hwnd)
    {
        if (hwnd == 0 || !IsWindow(hwnd))
            return string.Empty;

        var sb = new StringBuilder(256);
        _ = GetClassName(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    internal static uint GetProcessId(nint hwnd)
    {
        if (hwnd == 0 || !IsWindow(hwnd))
            return 0;
        return GetWindowThreadProcessId(hwnd, out uint pid) ? pid : 0;
    }

    internal static string TryGetWindowTitleOrEmpty(nint hwnd) => TryGetWindowText(hwnd);

    internal static (int Left, int Top, int Right, int Bottom, int Width, int Height) ToRectDto(Rect rect)
    {
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        return (rect.Left, rect.Top, rect.Right, rect.Bottom, width, height);
    }

    internal static int GetSystemDpiX()
    {
        try
        {
            // 可能在部分系统上不可用/失败；因此用 try/catch 并 fallback。
            return (int)GetDpiForSystem();
        }
        catch
        {
            nint hdc = GetDC(0);
            if (hdc == 0)
                return 96;
            try
            {
                return GetDeviceCaps(hdc, LogPixelsX);
            }
            finally
            {
                _ = ReleaseDC(0, hdc);
            }
        }
    }

    internal static int GetSystemDpiY()
    {
        try
        {
            return (int)GetDpiForSystem();
        }
        catch
        {
            nint hdc = GetDC(0);
            if (hdc == 0)
                return 96;
            try
            {
                return GetDeviceCaps(hdc, LogPixelsY);
            }
            finally
            {
                _ = ReleaseDC(0, hdc);
            }
        }
    }

    internal static bool TryGetSystemCursorPosition(out Point point)
    {
        point = default;
        return GetCursorPos(out point);
    }

    internal static string TryGetWindowTextForDebug(nint hwnd)
    {
        string t = TryGetWindowText(hwnd);
        if (string.IsNullOrEmpty(t))
            return string.Empty;
        // 避免返回超长字符串影响响应。
        return t.Length <= 1024 ? t : t[..1024];
    }
}

