using WinEventHook;

namespace Swg.Capture;

/// <summary>
/// WinEvent（SetWinEventHook）到窗口 <see cref="WindowCaptureEventTypes"/> 的映射；同一 WinEvent 可对应多个产品类型（与订阅求交后分别上报）。
/// </summary>
internal static class WinEventWindowCaptureMap
{
    /// <summary>根据订阅集合计算需要监听的 <see cref="WindowEvent"/> 集合。</summary>
    internal static HashSet<WindowEvent> CollectWinEvents(IReadOnlySet<string> subscribed)
    {
        var set = new HashSet<WindowEvent>();
        foreach ((WindowEvent w, string[] types) in Mappings)
        {
            foreach (string t in types)
            {
                if (subscribed.Contains(t))
                {
                    _ = set.Add(w);
                    break;
                }
            }
        }

        return set;
    }

    /// <summary>将发生的 WinEvent 展开为应推送的产品 EventType（已与订阅求交）。</summary>
    internal static IEnumerable<string> Expand(WindowEvent winEvent, IReadOnlySet<string> subscribed)
    {
        foreach ((WindowEvent w, string[] types) in Mappings)
        {
            if (w != winEvent)
                continue;
            foreach (string t in types)
            {
                if (subscribed.Contains(t))
                    yield return t;
            }

            yield break;
        }
    }

    /// <summary>
    /// 将若干 WinEvent 合并为最少连续区间（uint），用于构造多个 <see cref="WindowEventHook"/>。
    /// </summary>
    internal static List<(WindowEvent Min, WindowEvent Max)> ToRanges(HashSet<WindowEvent> events)
    {
        if (events.Count == 0)
            return new List<(WindowEvent, WindowEvent)>();

        uint[] sorted = events.Select(static e => (uint)e).Distinct().OrderBy(static x => x).ToArray();
        var ranges = new List<(uint Min, uint Max)>();
        uint rMin = sorted[0];
        uint rMax = sorted[0];
        for (int i = 1; i < sorted.Length; i++)
        {
            if (sorted[i] == rMax + 1)
            {
                rMax = sorted[i];
            }
            else
            {
                ranges.Add((rMin, rMax));
                rMin = sorted[i];
                rMax = sorted[i];
            }
        }

        ranges.Add((rMin, rMax));
        return ranges.ConvertAll(static x => ((WindowEvent)x.Min, (WindowEvent)x.Max));
    }

    /// <summary>映射表；随 Win32 文档与实测可调整。</summary>
    private static readonly (WindowEvent Win, string[] Types)[] Mappings =
    {
        (WindowEvent.EVENT_OBJECT_CREATE, new[] { WindowCaptureEventTypes.WindowCreated }),
        (WindowEvent.EVENT_OBJECT_DESTROY, new[] { WindowCaptureEventTypes.WindowClosed }),
        (WindowEvent.EVENT_OBJECT_SHOW, new[] { WindowCaptureEventTypes.WindowOpened }),
        (WindowEvent.EVENT_OBJECT_HIDE, new[] { WindowCaptureEventTypes.WindowHidden }),
        (WindowEvent.EVENT_SYSTEM_FOREGROUND, new[] { WindowCaptureEventTypes.WindowActivated }),
        (WindowEvent.EVENT_OBJECT_NAMECHANGE, new[] { WindowCaptureEventTypes.WindowTitleChanged }),
        (WindowEvent.EVENT_SYSTEM_MINIMIZESTART, new[] { WindowCaptureEventTypes.WindowMinimizeStart }),
        (WindowEvent.EVENT_SYSTEM_MINIMIZEEND, new[] { WindowCaptureEventTypes.WindowMinimizeEnd }),
        (WindowEvent.EVENT_OBJECT_LOCATIONCHANGE, new[]
        {
            WindowCaptureEventTypes.WindowMoved,
            WindowCaptureEventTypes.WindowResized,
        }),
        (WindowEvent.EVENT_OBJECT_FOCUS, new[] { WindowCaptureEventTypes.WindowFocused }),
        (WindowEvent.EVENT_OBJECT_STATECHANGE, new[]
        {
            WindowCaptureEventTypes.WindowDeactivated,
            WindowCaptureEventTypes.WindowEnabled,
            WindowCaptureEventTypes.WindowDisabled,
            WindowCaptureEventTypes.WindowMaximized,
            WindowCaptureEventTypes.WindowRestored,
            WindowCaptureEventTypes.WindowBlurred,
        }),
    };
}
