namespace Swg.Win32;

/// <summary>
/// 消息发送与组合键相关静态业务函数。
/// </summary>
public static class SwgWin32Messages
{
    public static long SendWindowMessage(string? windowHandle, uint msg, string? wParam, string? lParam)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(windowHandle, nameof(windowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Window handle invalid.");

        nint wp = string.IsNullOrWhiteSpace(wParam) ? 0 : Win32Native.ParseNintOrThrow(wParam, nameof(wParam));
        nint lp = string.IsNullOrWhiteSpace(lParam) ? 0 : Win32Native.ParseNintOrThrow(lParam, nameof(lParam));

        nint r = Win32Native.SendMessage(hwnd, msg, wp, lp);
        return r.ToInt64();
    }

    public static void PostWindowMessage(string? windowHandle, uint msg, string? wParam, string? lParam)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(windowHandle, nameof(windowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Window handle invalid.");

        nint wp = string.IsNullOrWhiteSpace(wParam) ? 0 : Win32Native.ParseNintOrThrow(wParam, nameof(wParam));
        nint lp = string.IsNullOrWhiteSpace(lParam) ? 0 : Win32Native.ParseNintOrThrow(lParam, nameof(lParam));

        bool ok = Win32Native.PostMessage(hwnd, msg, wp, lp);
        if (!ok)
            throw new InvalidOperationException("PostMessage failed.");
    }

    public static string GetWindowHandleAtPoint(int x, int y)
    {
        var p = new Win32Native.Point { X = x, Y = y };
        nint hwnd = Win32Native.WindowFromPoint(p);
        if (hwnd == 0)
            throw new InvalidOperationException("No window at point.");
        return Win32Native.FormatHandle(hwnd);
    }

    public static void SendKeysToWindow(string? windowHandle, IReadOnlyList<string>? keys)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(windowHandle, nameof(windowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Window handle invalid.");

        if (keys is null || keys.Count == 0)
            throw new ArgumentException("Keys 不能为空。", nameof(keys));

        // modifier keys: Ctrl/Alt/Shift/Win
        var modifierVks = new List<uint>(4);
        var modifierVkSet = new HashSet<uint>();
        var mainKeys = new List<ParsedMainKey>();

        foreach (string rawKey in keys)
        {
            if (string.IsNullOrWhiteSpace(rawKey))
                throw new ArgumentException("Keys 内存在空 key。");

            string k = rawKey.Trim();
            if (TryParseModifierVk(k, out uint modVk))
            {
                if (modifierVkSet.Add(modVk))
                    modifierVks.Add(modVk);
                continue;
            }

            mainKeys.Add(ParseMainKey(k));
        }

        if (mainKeys.Count == 0)
            throw new ArgumentException("Keys 必须包含至少一个主键（非 modifier）。");

        // 先按 modifier
        foreach (uint modVk in modifierVks)
        {
            SendKeyDown(hwnd, modVk);
        }

        // 主键依次按下/抬起
        foreach (var mk in mainKeys)
        {
            SendKeyDown(hwnd, mk.Vk);
            if (mk.CharToSend != null)
                SendWindowMessage(hwnd, Win32Native.WmChar, (nint)mk.CharToSend.Value, 0);
            SendKeyUp(hwnd, mk.Vk);
        }

        // 最后释放 modifier（反向）
        for (int i = modifierVks.Count - 1; i >= 0; i--)
        {
            SendKeyUp(hwnd, modifierVks[i]);
        }
    }

    private readonly record struct ParsedMainKey(uint Vk, char? CharToSend);

    private static bool TryParseModifierVk(string key, out uint vk)
    {
        string k = key.Trim();
        if (k.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("Control", StringComparison.OrdinalIgnoreCase))
        {
            vk = Win32Native.VkControl;
            return true;
        }

        if (k.Equals("Alt", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("Menu", StringComparison.OrdinalIgnoreCase))
        {
            vk = Win32Native.VkMenu;
            return true;
        }

        if (k.Equals("Shift", StringComparison.OrdinalIgnoreCase))
        {
            vk = Win32Native.VkShift;
            return true;
        }

        if (k.Equals("Win", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("Windows", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("Super", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("Meta", StringComparison.OrdinalIgnoreCase))
        {
            vk = Win32Native.VkLWin;
            return true;
        }

        vk = 0;
        return false;
    }

    private static ParsedMainKey ParseMainKey(string key)
    {
        string k = key.Trim();
        if (k.Length == 1)
        {
            char c = k[0];
            if (c is >= 'a' and <= 'z')
            {
                char upper = char.ToUpperInvariant(c);
                return new ParsedMainKey((uint)upper, upper);
            }
            if (c is >= 'A' and <= 'Z')
                return new ParsedMainKey((uint)c, c);
            if (c is >= '0' and <= '9')
                return new ParsedMainKey((uint)c, c);
        }

        string lower = k.ToLowerInvariant();
        return lower switch
        {
            "up" => new ParsedMainKey(0x26, null),
            "down" => new ParsedMainKey(0x28, null),
            "left" => new ParsedMainKey(0x25, null),
            "right" => new ParsedMainKey(0x27, null),

            "home" => new ParsedMainKey(0x24, null),
            "end" => new ParsedMainKey(0x23, null),
            "pageup" or "page_up" => new ParsedMainKey(0x21, null),
            "pagedown" or "page_down" => new ParsedMainKey(0x22, null),
            "insert" => new ParsedMainKey(0x2D, null),
            "delete" => new ParsedMainKey(0x2E, null),
            "backspace" => new ParsedMainKey(0x08, null),
            "tab" => new ParsedMainKey(0x09, null),
            "enter" or "return" => new ParsedMainKey(0x0D, null),
            "escape" or "esc" => new ParsedMainKey(0x1B, null),
            "space" => new ParsedMainKey(0x20, ' '),

            _ when lower.StartsWith("f") && int.TryParse(lower[1..], out int idx) && idx >= 1 && idx <= 12 =>
                new ParsedMainKey(0x6Fu + (uint)idx, null),

            _ => throw new ArgumentException($"不支持的主键: {key}。"),
        };
    }

    private static void SendKeyDown(nint hwnd, uint vk)
    {
        nint lParam = BuildKeyLParam(vk);
        _ = Win32Native.SendMessage(hwnd, Win32Native.WmKeyDown, (nint)vk, lParam);
    }

    private static void SendKeyUp(nint hwnd, uint vk)
    {
        nint lParam = BuildKeyLParam(vk);
        _ = Win32Native.SendMessage(hwnd, Win32Native.WmKeyUp, (nint)vk, lParam);
    }

    private static nint BuildKeyLParam(uint vk)
    {
        uint scan = Win32Native.MapVirtualKey(vk, 0);
        // Windows 的 lParam: scanCode 在高 16 位；这里给最基本值即可。
        return (nint)((int)(scan << 16));
    }

    private static long SendWindowMessage(nint hwnd, uint msg, nint wParam, nint lParam)
    {
        nint r = Win32Native.SendMessage(hwnd, msg, wParam, lParam);
        return r.ToInt64();
    }
}

