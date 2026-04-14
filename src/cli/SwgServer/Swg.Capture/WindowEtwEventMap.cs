namespace Swg.Capture;

/// <summary>
/// ETW Provider/EventName 到窗口 <see cref="WindowCaptureEventTypes"/> 的弱映射（随 Windows 版本需回归；无法匹配则不上报）。
/// </summary>
internal static class WindowEtwEventMap
{
    /// <summary>
    /// 返回与 <paramref name="subscribed"/> 相交后的产品事件类型（可能多项，如 Moved+Resized 同指纹路）。
    /// </summary>
    internal static List<string> TryMap(string providerName, string eventName, IReadOnlySet<string> subscribed)
    {
        string p = providerName ?? "";
        string e = (eventName ?? "").Trim();
        if (e.Length == 0)
            return new List<string>();

        var hits = new List<string>();

        // 通用启发式（不绑定具体 EventId，随 OS 更新风险较低）
        void AddIfSubscribed(string type)
        {
            if (subscribed.Contains(type))
                hits.Add(type);
        }

        if (ContainsIgnoreCase(e, "Create") || ContainsIgnoreCase(e, "Created"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowCreated);
        if (ContainsIgnoreCase(e, "Destroy") || ContainsIgnoreCase(e, "Destroyed") || ContainsIgnoreCase(e, "Close"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowClosed);
        if (ContainsIgnoreCase(e, "Show") || ContainsIgnoreCase(e, "Visible"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowOpened);
        if (ContainsIgnoreCase(e, "Hide") || ContainsIgnoreCase(e, "Hidden"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowHidden);
        if (ContainsIgnoreCase(e, "Foreground") || ContainsIgnoreCase(e, "Activate") || ContainsIgnoreCase(e, "Active"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowActivated);
        if (ContainsIgnoreCase(e, "Deactivate") || ContainsIgnoreCase(e, "Inactive"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowDeactivated);
        if (ContainsIgnoreCase(e, "Title") || ContainsIgnoreCase(e, "Name"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowTitleChanged);
        if (ContainsIgnoreCase(e, "Minimize"))
        {
            AddIfSubscribed(WindowCaptureEventTypes.WindowMinimizeStart);
            AddIfSubscribed(WindowCaptureEventTypes.WindowMinimizeEnd);
        }

        if (ContainsIgnoreCase(e, "Maximize"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowMaximized);
        if (ContainsIgnoreCase(e, "Restore") || ContainsIgnoreCase(e, "Restored"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowRestored);
        if (ContainsIgnoreCase(e, "Move") || ContainsIgnoreCase(e, "Position"))
        {
            AddIfSubscribed(WindowCaptureEventTypes.WindowMoved);
            AddIfSubscribed(WindowCaptureEventTypes.WindowResized);
        }

        if (ContainsIgnoreCase(e, "Resize") || ContainsIgnoreCase(e, "Size"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowResized);
        if (ContainsIgnoreCase(e, "Enable"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowEnabled);
        if (ContainsIgnoreCase(e, "Disable"))
            AddIfSubscribed(WindowCaptureEventTypes.WindowDisabled);
        if (ContainsIgnoreCase(e, "Focus"))
        {
            AddIfSubscribed(WindowCaptureEventTypes.WindowFocused);
            AddIfSubscribed(WindowCaptureEventTypes.WindowBlurred);
        }

        return hits.Distinct(StringComparer.Ordinal).ToList();
    }

    private static bool ContainsIgnoreCase(string haystack, string needle) =>
        haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
