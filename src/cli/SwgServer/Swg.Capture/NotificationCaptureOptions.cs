namespace Swg.Capture;

/// <summary>
/// 通知捕获：去抖、进程过滤等（与 WinEventHook 配合）。
/// </summary>
public sealed class NotificationCaptureOptions
{
    /// <summary>
    /// Hook 路径订阅的窗口 <see cref="WindowCaptureEventTypes"/> 子集；为 <c>null</c> 时使用 <see cref="WindowCaptureEventTypes.DefaultHookSubscription"/>；
    /// 为空列表表示显式不注册任何窗口 WinEvent。
    /// </summary>
    public IReadOnlyList<string>? HookWindowEventTypes { get; set; }

    /// <summary>相同去抖键（DebounceKey）的最小间隔（毫秒）。</summary>
    public int DebounceMs { get; set; } = 2000;

    /// <summary>若非空，仅当进程名包含任一子串时上报（忽略大小写）。</summary>
    public IReadOnlyList<string>? ProcessNameContains { get; set; }

    /// <summary>可选：仅处理窗口标题包含该子串的事件（忽略大小写）。</summary>
    public string? TitleContains { get; set; }
}
