using System;

namespace Swg.Input;

/// <summary>
/// 键名到 VK（Virtual Key，虚拟键）映射。
/// </summary>
public static class InputKeyMap
{
    public const ushort VkCtrl = 0x11;
    public const ushort VkAlt = 0x12;
    public const ushort VkShift = 0x10;
    public const ushort VkWin = 0x5B;

    /// <summary>
    /// 尝试解析键名为 VK。
    /// </summary>
    public static bool TryParseKeyName(string? name, out ushort vk)
    {
        vk = 0;
        if (string.IsNullOrWhiteSpace(name))
            return false;

        string k = name.Trim();

        // 单字符：字母/数字直接映射到对应字符码（字母统一转大写）。
        if (k.Length == 1)
        {
            char c = k[0];
            if (c is >= 'a' and <= 'z')
            {
                vk = (ushort)char.ToUpperInvariant(c);
                return true;
            }
            if (c is >= 'A' and <= 'Z')
            {
                vk = (ushort)c;
                return true;
            }
            if (c is >= '0' and <= '9')
            {
                vk = (ushort)c;
                return true;
            }

            // 其他单字符不支持（例如符号）。
            return false;
        }

        string lower = k.ToLowerInvariant();

        vk = lower switch
        {
            "up" => 0x26,
            "down" => 0x28,
            "left" => 0x25,
            "right" => 0x27,

            "home" => 0x24,
            "end" => 0x23,
            "pageup" or "page_up" => 0x21,
            "pagedown" or "page_down" => 0x22,

            "insert" => 0x2D,
            "delete" => 0x2E,
            "backspace" => 0x08,
            "tab" => 0x09,
            "enter" or "return" => 0x0D,
            "escape" or "esc" => 0x1B,
            "space" => 0x20,

            _ => TryParseFunctionKey(lower, out ushort fnVk) ? fnVk : (ushort)0,
        };

        if (vk != 0)
            return true;

        // 额外：支持“F1..F12”（上面 _ 分支实际已尝试，保留兜底便于调试）。
        return false;
    }

    public static ushort ParseKeyNameOrThrow(string? name)
    {
        if (!TryParseKeyName(name, out ushort vk))
            throw new ArgumentException($"无法解析的按键名：{name}。", nameof(name));
        return vk;
    }

    private static bool TryParseFunctionKey(string lower, out ushort vk)
    {
        vk = 0;
        if (!lower.StartsWith('f'))
            return false;
        if (!int.TryParse(lower[1..], out int idx))
            return false;
        if (idx < 1 || idx > 12)
            return false;

        vk = (ushort)(0x6Fu + idx);
        return true;
    }
}

