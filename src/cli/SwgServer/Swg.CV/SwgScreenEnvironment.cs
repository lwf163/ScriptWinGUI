using System.Runtime.InteropServices;

namespace Swg.CV;

/// <summary>
/// 宿主进程显示环境：统一 DPI 语义与虚拟桌面参考原点，使 <c>Swg.CV</c>（Win32/GDI）与 <c>Swg.FlaUI</c>（UIA）在同一进程内使用一致坐标系。
/// </summary>
public static class SwgScreenEnvironment
{
    private const int SmXVirtualScreen = 76;
    private const int SmYVirtualScreen = 77;
    private const int SmCxVirtualScreen = 78;
    private const int SmCyVirtualScreen = 79;

    /// <summary>
    /// DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2（句柄值 -4）。
    /// </summary>
    private static readonly nint PerMonitorAwareV2 = (nint)(-4);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetProcessDpiAwarenessContext(nint dpiContext);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetSystemMetrics(int nIndex);

    /// <summary>
    /// 在进程入口<strong>最早</strong>调用：将本进程设为 Per-Monitor V2 DPI 感知（与清单声明互补）。
    /// </summary>
    /// <returns>是否调用成功（旧系统或不支持时可能为 <c>false</c>，此时应依赖应用清单）。</returns>
    public static bool TrySetPerMonitorV2DpiAwareness()
    {
        return SetProcessDpiAwarenessContext(PerMonitorAwareV2);
    }

    /// <summary>
    /// 虚拟桌面外接矩形（逻辑像素），坐标原点为虚拟桌面左上角；截屏与 <c>GetWindowRect</c> 均应与此坐标系对齐。
    /// </summary>
    public static VirtualDesktopMetrics GetVirtualDesktopMetrics()
    {
        int x = GetSystemMetrics(SmXVirtualScreen);
        int y = GetSystemMetrics(SmYVirtualScreen);
        int w = GetSystemMetrics(SmCxVirtualScreen);
        int h = GetSystemMetrics(SmCyVirtualScreen);
        return new VirtualDesktopMetrics(x, y, w, h);
    }

    /// <summary>
    /// 建议宿主（如 <c>SwgServer</c>）在 <c>Main</c> 第一行调用：设置 DPI 感知并可选记录虚拟桌面范围。
    /// </summary>
    /// <param name="log">为 <c>null</c> 时不输出；否则写入一行摘要。</param>
    public static void Initialize(Action<string>? log = null)
    {
        bool dpiOk = TrySetPerMonitorV2DpiAwareness();
        var vd = GetVirtualDesktopMetrics();
        log?.Invoke(
            string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "[SwgScreenEnvironment] PerMonitorV2={0}, VirtualDesktop=({1},{2}) {3}x{4}",
                dpiOk,
                vd.X,
                vd.Y,
                vd.Width,
                vd.Height));
    }
}

/// <summary>虚拟桌面范围（与 <c>SM_*VIRTUALSCREEN</c> 一致）。</summary>
public readonly record struct VirtualDesktopMetrics(int X, int Y, int Width, int Height);
