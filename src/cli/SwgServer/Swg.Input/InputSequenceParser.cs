using System;
using System.Collections.Generic;

namespace Swg.Input;

/// <summary>
/// 序列 token 的解析结果：一组修饰键 + 一个主键（VK）。
/// </summary>
public sealed record InputSequenceToken(IReadOnlyList<ushort> Modifiers, ushort MainVk);

/// <summary>
/// 输入序列解析器：按逗号 token 分割，解析修饰符前缀（^!+#+）与主键。
/// </summary>
public static class InputSequenceParser
{
    /// <summary>
    /// 解析输入序列。
    /// </summary>
    public static IReadOnlyList<InputSequenceToken> Parse(string? sequence)
    {
        if (string.IsNullOrWhiteSpace(sequence))
            throw new ArgumentException("Sequence 必填。", nameof(sequence));

        var rawTokens = sequence.Split(new[] { ',', '，' }, StringSplitOptions.None);
        var result = new List<InputSequenceToken>(rawTokens.Length);

        foreach (string raw in rawTokens)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentException($"Sequence token 不能为空：{sequence}。");

            string token = raw.Trim();
            result.Add(ParseToken(token));
        }

        if (result.Count == 0)
            throw new ArgumentException("Sequence 至少包含一个 token。", nameof(sequence));

        return result;
    }

    private static InputSequenceToken ParseToken(string token)
    {
        int i = 0;
        var modifiers = new List<ushort>(4);
        var seen = new HashSet<ushort>();

        // 解析前缀修饰符：^ / ! / + / #
        while (i < token.Length)
        {
            char ch = token[i];
            ushort? vk = ch switch
            {
                '^' => InputKeyMap.VkCtrl,
                '!' => InputKeyMap.VkAlt,
                '+' => InputKeyMap.VkShift,
                '#' => InputKeyMap.VkWin,
                _ => null,
            };

            if (vk is null)
                break;

            if (seen.Add(vk.Value))
                modifiers.Add(vk.Value);

            i++;
        }

        string mainKeyName = token[i..].Trim();
        if (string.IsNullOrWhiteSpace(mainKeyName))
            throw new ArgumentException($"token 缺少主键：{token}。", nameof(token));

        ushort mainVk = InputKeyMap.ParseKeyNameOrThrow(mainKeyName);
        return new InputSequenceToken(modifiers, mainVk);
    }
}

