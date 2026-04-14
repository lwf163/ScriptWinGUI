namespace Swg.Capture;

/// <summary>
/// 窗口类通知 <c>EventType</c> 单源目录（Hook 与 ETW 窗口轨共用字符串与语义）。
/// </summary>
public static class WindowCaptureEventTypes
{
    public const string WindowCreated = "WindowCreated";
    public const string WindowOpened = "WindowOpened";
    public const string WindowActivated = "WindowActivated";
    public const string WindowClosed = "WindowClosed";
    public const string WindowHidden = "WindowHidden";
    public const string WindowDeactivated = "WindowDeactivated";
    public const string WindowTitleChanged = "WindowTitleChanged";
    public const string WindowMinimizeStart = "WindowMinimizeStart";
    public const string WindowMinimizeEnd = "WindowMinimizeEnd";
    public const string WindowMaximized = "WindowMaximized";
    public const string WindowRestored = "WindowRestored";
    public const string WindowMoved = "WindowMoved";
    public const string WindowResized = "WindowResized";
    public const string WindowEnabled = "WindowEnabled";
    public const string WindowDisabled = "WindowDisabled";
    public const string WindowFocused = "WindowFocused";
    public const string WindowBlurred = "WindowBlurred";

    /// <summary>与创建监听窗口时「未指定 Hook 订阅」的向后兼容默认（原 SHOW + NAMECHANGE）。</summary>
    public static readonly IReadOnlyList<string> DefaultHookSubscription = new[]
    {
        WindowOpened,
        WindowTitleChanged,
    };

    private static readonly HashSet<string> AllSet = new(StringComparer.Ordinal)
    {
        WindowCreated,
        WindowOpened,
        WindowActivated,
        WindowClosed,
        WindowHidden,
        WindowDeactivated,
        WindowTitleChanged,
        WindowMinimizeStart,
        WindowMinimizeEnd,
        WindowMaximized,
        WindowRestored,
        WindowMoved,
        WindowResized,
        WindowEnabled,
        WindowDisabled,
        WindowFocused,
        WindowBlurred,
    };

    /// <summary>权威集合，供校验与文档对齐。</summary>
    public static IReadOnlyCollection<string> All => AllSet;

    /// <summary>若 <paramref name="types"/> 中任一项不在目录内则抛出 <see cref="ArgumentException"/>。</summary>
    public static void ValidateSubscription(IReadOnlyList<string>? types, string parameterName)
    {
        if (types is null || types.Count == 0)
            return;

        foreach (string t in types)
        {
            if (string.IsNullOrWhiteSpace(t))
                throw new ArgumentException("窗口事件类型不能为空字符串。", parameterName);

            if (!AllSet.Contains(t.Trim()))
                throw new ArgumentException($"未知的窗口事件类型: \"{t.Trim()}\"（须为 WindowCaptureEventTypes 目录中的值）。", parameterName);
        }
    }

    /// <summary>规范化：trim、去重（序保持）。</summary>
    public static IReadOnlyList<string> NormalizeSubscription(IReadOnlyList<string>? types)
    {
        if (types is null || types.Count == 0)
            return Array.Empty<string>();

        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string raw in types)
        {
            string s = raw.Trim();
            if (s.Length == 0)
                continue;
            if (seen.Add(s))
                list.Add(s);
        }

        return list;
    }
}
