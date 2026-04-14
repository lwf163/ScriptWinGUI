using System.Runtime.InteropServices;

namespace Swg.Win32;

/// <summary>
/// 系统信息静态业务函数。
/// </summary>
public static class SwgWin32SystemInfo
{
    private const int SmXVirtualScreen = 76;
    private const int SmYVirtualScreen = 77;
    private const int SmCxVirtualScreen = 78;
    private const int SmCyVirtualScreen = 79;

    private const int SmCxScreen = 0;
    private const int SmCyScreen = 1;

    public static MainScreenSize GetMainScreen()
    {
        int w = Win32Native.GetSystemMetrics(Win32Native.SmCxScreen);
        int h = Win32Native.GetSystemMetrics(Win32Native.SmCyScreen);
        if (w <= 0 || h <= 0)
        {
            int err = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"GetSystemMetrics(SCREEN) failed. Win32Error={err}.");
        }
        return new MainScreenSize(w, h);
    }

    public static VirtualScreenBounds GetVirtualScreen()
    {
        int x = Win32Native.GetSystemMetrics(SmXVirtualScreen);
        int y = Win32Native.GetSystemMetrics(SmYVirtualScreen);
        int w = Win32Native.GetSystemMetrics(SmCxVirtualScreen);
        int h = Win32Native.GetSystemMetrics(SmCyVirtualScreen);
        if (w <= 0 || h <= 0)
        {
            int err = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"GetSystemMetrics(VIRTUALSCREEN) failed. Win32Error={err}.");
        }
        return new VirtualScreenBounds(x, y, w, h);
    }

    public static SystemDpi GetSystemDpi()
    {
        int dx = Win32Native.GetSystemDpiX();
        int dy = Win32Native.GetSystemDpiY();
        return new SystemDpi(dx, dy);
    }

    public static ScreenCursorPoint GetCursorPosition()
    {
        if (!Win32Native.TryGetSystemCursorPosition(out var p))
        {
            int err = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"GetCursorPos failed. Win32Error={err}.");
        }
        return new ScreenCursorPoint(p.X, p.Y);
    }

    public static ForegroundWindowInfo GetForegroundWindowInfo()
    {
        string hwnd = SwgWin32Window.GetForegroundWindowHandle();
        WindowInfo info = SwgWin32Window.GetWindowInfo(hwnd);
        return new ForegroundWindowInfo(info);
    }
}

