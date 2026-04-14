using System;
using System.Collections.Generic;
using Swg.Win32;

namespace Swg.Input;

/// <summary>
/// 键盘输入模拟。
/// </summary>
public static class InputKeyboard
{
    /// <summary>按文本输入（Unicode）。</summary>
    public static void TypeText(string? text)
    {
        if (text is null)
            throw new ArgumentException("Text 必填。", nameof(text));

        if (text.Length == 0)
            return;

        // 计划里强调“批量 SendInput + buffer flush”，减少系统调用次数并提升时序稳定性。
        SwgWin32Input.SendKeyboardUnicodeCharsBatch(text.AsSpan());
        // 短等待：让系统输入队列有机会处理这批注入事件。
        InputWait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(50));
    }

    /// <summary>按单个字符输入（长度语义由上层保证）。</summary>
    public static void TypeChar(char c)
    {
        // Unicode 字符注入：keydown + keyup。
        SwgWin32Input.SendKeyboardUnicodeChar(c, isDown: true);
        SwgWin32Input.SendKeyboardUnicodeChar(c, isDown: false);
    }

    /// <summary>依次按下/抬起一组按键。</summary>
    public static void TypeKeys(IReadOnlyList<string>? keys)
    {
        if (keys is null || keys.Count == 0)
            throw new ArgumentException("Keys 不能为空。", nameof(keys));

        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Keys 内存在空 key。", nameof(keys));
            TypeKey(key);
        }
    }

    /// <summary>同时按下多个按键（先按先放）。</summary>
    public static void TypeSimultaneously(IReadOnlyList<string>? keys)
    {
        if (keys is null || keys.Count == 0)
            throw new ArgumentException("Keys 不能为空。", nameof(keys));

        var vks = new ushort[keys.Count];
        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Keys 内存在空 key。", nameof(keys));
            vks[i] = InputKeyMap.ParseKeyNameOrThrow(key);
        }

        // 先按（按顺序）。
        for (int i = 0; i < vks.Length; i++)
        {
            SwgWin32Input.SendKeyboardVirtualKey(vks[i], isDown: true);
        }

        // 再放（先按先放：保持与按下相同的顺序）。
        for (int i = 0; i < vks.Length; i++)
        {
            SwgWin32Input.SendKeyboardVirtualKey(vks[i], isDown: false);
        }
    }

    /// <summary>按键序列输入（Sequence：逗号 token 分割；修饰前缀 ^!+#+）。</summary>
    public static void TypeSequence(string? sequence)
    {
        var tokens = InputSequenceParser.Parse(sequence);
        foreach (var token in tokens)
        {
            // 修饰键先按（token.Modifiers 保持原始顺序）。
            foreach (var vk in token.Modifiers)
            {
                SwgWin32Input.SendKeyboardVirtualKey(vk, isDown: true);
            }

            // 主键按下 + 抬起。
            SwgWin32Input.SendKeyboardVirtualKey(token.MainVk, isDown: true);
            SwgWin32Input.SendKeyboardVirtualKey(token.MainVk, isDown: false);

            // 修饰键反向释放。
            for (int i = token.Modifiers.Count - 1; i >= 0; i--)
            {
                SwgWin32Input.SendKeyboardVirtualKey(token.Modifiers[i], isDown: false);
            }
        }
    }

    /// <summary>发送单个按键（按下+抬起）。</summary>
    public static void TypeKey(string? keyName)
    {
        ushort vk = InputKeyMap.ParseKeyNameOrThrow(keyName);
        SwgWin32Input.SendKeyboardVirtualKey(vk, isDown: true);
        SwgWin32Input.SendKeyboardVirtualKey(vk, isDown: false);
    }

    /// <summary>按键按下（不抬起）。</summary>
    public static void PressKey(string? keyName)
    {
        ushort vk = InputKeyMap.ParseKeyNameOrThrow(keyName);
        SwgWin32Input.SendKeyboardVirtualKey(vk, isDown: true);
    }

    /// <summary>按键抬起。</summary>
    public static void ReleaseKey(string? keyName)
    {
        ushort vk = InputKeyMap.ParseKeyNameOrThrow(keyName);
        SwgWin32Input.SendKeyboardVirtualKey(vk, isDown: false);
    }
}

