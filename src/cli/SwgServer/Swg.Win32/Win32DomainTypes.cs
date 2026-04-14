namespace Swg.Win32;

/// <summary>窗口矩形（领域值类型，与 HTTP DTO 解耦）。</summary>
public sealed record WindowRect(int Left, int Top, int Right, int Bottom, int Width, int Height);

/// <summary>窗口信息（领域值类型）。</summary>
public sealed record WindowInfo(
    string WindowHandle,
    string Title,
    string ClassName,
    uint ProcessId,
    WindowRect Rect);

/// <summary>前台窗口信息（领域值类型）。</summary>
public sealed record ForegroundWindowInfo(WindowInfo Window);

/// <summary>主显示器像素尺寸。</summary>
public sealed record MainScreenSize(int Width, int Height);

/// <summary>虚拟屏幕边界（与 Win32 虚拟屏指标一致）。</summary>
public sealed record VirtualScreenBounds(int X, int Y, int Width, int Height);

/// <summary>系统 DPI。</summary>
public sealed record SystemDpi(int DpiX, int DpiY);

/// <summary>屏幕坐标系下的光标位置（与 GetCursorPos 一致）。</summary>
public sealed record ScreenCursorPoint(int X, int Y);
