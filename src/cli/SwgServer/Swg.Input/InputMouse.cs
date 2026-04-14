using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Swg.Win32;

namespace Swg.Input;

/// <summary>
/// 鼠标输入模拟（使用 Virtual Desktop 坐标系注入）。
/// </summary>
public static class InputMouse
{
    private static readonly object MoveSettingsLock = new();

    private static double MovePixelsPerMillisecond { get; set; } = 0.5;
    private static double MovePixelsPerStep { get; set; } = 10;

    private const int RandomJitterMaxPixels = 1;

    /// <summary>获取鼠标当前位置（转换到虚拟桌面坐标系）。</summary>
    public static VirtualDesktopCursorPosition GetCursorPosition()
    {
        var vd = SwgWin32SystemInfo.GetVirtualScreen();
        var c = SwgWin32SystemInfo.GetCursorPosition();

        int x = c.X - vd.X;
        int y = c.Y - vd.Y;
        return new VirtualDesktopCursorPosition(x, y);
    }

    /// <summary>设置鼠标位置（不做渐变）。</summary>
    public static void SetCursorPosition(int x, int y)
    {
        var vd = SwgWin32SystemInfo.GetVirtualScreen();
        ValidateVirtualDesktopPoint(x, y, vd);
        SwgWin32Input.SetCursorPositionVirtualDesktop(x, y, vd);
    }

    /// <summary>渐变移动到指定虚拟桌面坐标。</summary>
    public static void MoveTo(int x, int y)
    {
        var vd = SwgWin32SystemInfo.GetVirtualScreen();
        ValidateVirtualDesktopPoint(x, y, vd);

        var start = GetCursorPosition();
        int startX = start.X;
        int startY = start.Y;
        if (startX == x && startY == y)
            return;

        double durationMs;
        double stepDistance;
        lock (MoveSettingsLock)
        {
            if (MovePixelsPerMillisecond <= 0)
                throw new InvalidOperationException("MovePixelsPerMillisecond 必须大于 0。");
            if (MovePixelsPerStep <= 0)
                throw new InvalidOperationException("MovePixelsPerStep 必须大于 0。");

            // 仅 moveTo/drag 的渐变插值速率由该速度设置控制。
            double totalDistance = Distance(startX, startY, x, y);
            durationMs = totalDistance / MovePixelsPerMillisecond;
            stepDistance = MovePixelsPerStep;
        }

        int steps = Math.Max((int)(Distance(startX, startY, x, y) / stepDistance), 1);
        double intervalMs = Math.Max(durationMs / steps, 0);

        for (int i = 1; i < steps; i++)
        {
            double t = i / (double)steps;
            double eased = EaseInOut(t);

            int nx = (int)Math.Round(startX + (x - startX) * eased);
            int ny = (int)Math.Round(startY + (y - startY) * eased);

            // 随机抖动：避免固定轨迹（仅影响中间点，终点不抖动）。
            if (RandomJitterMaxPixels > 0)
            {
                nx += Random.Shared.Next(-RandomJitterMaxPixels, RandomJitterMaxPixels + 1);
                ny += Random.Shared.Next(-RandomJitterMaxPixels, RandomJitterMaxPixels + 1);
            }

            nx = Math.Clamp(nx, 0, vd.Width - 1);
            ny = Math.Clamp(ny, 0, vd.Height - 1);
            SwgWin32Input.SetCursorPositionVirtualDesktop(nx, ny, vd);

            if (intervalMs >= 1)
                Thread.Sleep((int)intervalMs);
        }

        // 确保精确到达终点。
        SwgWin32Input.SetCursorPositionVirtualDesktop(x, y, vd);
        // 与 FlaUI 的 Wait.UntilInputIsProcessed 行为一致：在移动完成后做一次输入队列缓冲。
        InputWait.UntilInputIsProcessed();
    }

    /// <summary>相对移动。</summary>
    public static void MoveBy(int deltaX, int deltaY)
    {
        var cur = GetCursorPosition();
        MoveTo(cur.X + deltaX, cur.Y + deltaY);
    }

    /// <summary>点击（不抬起前先执行可选定位）。</summary>
    public static void Click(int? x, int? y, SwgWin32Input.MouseButton button, int clickCount)
    {
        if (clickCount <= 0)
            throw new ArgumentException("clickCount 必须大于 0。", nameof(clickCount));

        if (x.HasValue || y.HasValue)
        {
            if (!x.HasValue || !y.HasValue)
                throw new ArgumentException("Click 需要同时提供 X 与 Y（或都为空）。");

            MoveTo(x.Value, y.Value);
        }

        for (int i = 0; i < clickCount; i++)
        {
            SwgWin32Input.SendMouseButton(button, isDown: true);
            // 轻微等待以减少某些窗口对快速按下/抬起的吞并。
            InputWait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(20));
            SwgWin32Input.SendMouseButton(button, isDown: false);
            if (i < clickCount - 1)
                InputWait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(50));
        }
    }

    /// <summary>按下鼠标不抬起（支持修饰键保持）。</summary>
    public static void Press(int? x, int? y, SwgWin32Input.MouseButton button, IReadOnlyList<string>? modifiers)
    {
        if (x.HasValue || y.HasValue)
        {
            if (!x.HasValue || !y.HasValue)
                throw new ArgumentException("Down 需要同时提供 X 与 Y（或都为空）。");
            MoveTo(x.Value, y.Value);
        }

        var modVks = ParseModifiersToVks(modifiers);
        PressModifiers(modVks);
        SwgWin32Input.SendMouseButton(button, isDown: true);
    }

    /// <summary>抬起鼠标（释放修饰键，反向释放）。</summary>
    public static void Release(SwgWin32Input.MouseButton button, IReadOnlyList<string>? modifiers)
    {
        var modVks = ParseModifiersToVks(modifiers);
        SwgWin32Input.SendMouseButton(button, isDown: false);
        ReleaseModifiersReverse(modVks);
    }

    /// <summary>拖拽从起点到终点。</summary>
    public static void DragTo(
        int startX,
        int startY,
        int endX,
        int endY,
        SwgWin32Input.MouseButton button,
        IReadOnlyList<string>? modifiers)
    {
        var vd = SwgWin32SystemInfo.GetVirtualScreen();
        ValidateVirtualDesktopPoint(startX, startY, vd);
        ValidateVirtualDesktopPoint(endX, endY, vd);

        MoveTo(startX, startY);

        var modVks = ParseModifiersToVks(modifiers);
        PressModifiers(modVks);
        SwgWin32Input.SendMouseButton(button, isDown: true);
        InputWait.UntilInputIsProcessed();

        MoveTo(endX, endY);

        SwgWin32Input.SendMouseButton(button, isDown: false);
        InputWait.UntilInputIsProcessed();
        ReleaseModifiersReverse(modVks);
    }

    /// <summary>拖拽按距离。</summary>
    public static void DragByDistance(
        int startX,
        int startY,
        int distanceX,
        int distanceY,
        SwgWin32Input.MouseButton button,
        IReadOnlyList<string>? modifiers)
    {
        DragTo(startX, startY, startX + distanceX, startY + distanceY, button, modifiers);
    }

    /// <summary>滚动垂直方向。</summary>
    public static void ScrollAt(int x, int y, int wheelLines)
    {
        var vd = SwgWin32SystemInfo.GetVirtualScreen();
        ValidateVirtualDesktopPoint(x, y, vd);

        SwgWin32Input.SetCursorPositionVirtualDesktop(x, y, vd);
        InputWait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(20));
        SwgWin32Input.SendMouseWheel(wheelLines);
        InputWait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(20));
    }

    /// <summary>滚动水平方向。</summary>
    public static void HorizontalScrollAt(int x, int y, int wheelLines)
    {
        var vd = SwgWin32SystemInfo.GetVirtualScreen();
        ValidateVirtualDesktopPoint(x, y, vd);

        SwgWin32Input.SetCursorPositionVirtualDesktop(x, y, vd);
        InputWait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(20));
        SwgWin32Input.SendMouseHorizontalWheel(wheelLines);
        InputWait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(20));
    }

    /// <summary>获取移动速度设置。</summary>
    public static MouseMoveSettings GetMoveSettings()
    {
        lock (MoveSettingsLock)
        {
            return new MouseMoveSettings(MovePixelsPerMillisecond, MovePixelsPerStep);
        }
    }

    /// <summary>更新移动速度设置。</summary>
    public static void UpdateMoveSettings(double? movePixelsPerMillisecond, double? movePixelsPerStep)
    {
        lock (MoveSettingsLock)
        {
            if (movePixelsPerMillisecond is not null)
            {
                if (movePixelsPerMillisecond.Value <= 0)
                    throw new ArgumentException("MovePixelsPerMillisecond 必须大于 0。", nameof(movePixelsPerMillisecond));
                MovePixelsPerMillisecond = movePixelsPerMillisecond.Value;
            }

            if (movePixelsPerStep is not null)
            {
                if (movePixelsPerStep.Value <= 0)
                    throw new ArgumentException("MovePixelsPerStep 必须大于 0。", nameof(movePixelsPerStep));
                MovePixelsPerStep = movePixelsPerStep.Value;
            }
        }
    }

    private static void ValidateVirtualDesktopPoint(int x, int y, VirtualScreenBounds vd)
    {
        if (x < 0 || x >= vd.Width)
            throw new ArgumentException("坐标 x 越界（虚拟桌面坐标要求从 0 开始）。", nameof(x));
        if (y < 0 || y >= vd.Height)
            throw new ArgumentException("坐标 y 越界（虚拟桌面坐标要求从 0 开始）。", nameof(y));
    }

    private static double Distance(int x1, int y1, int x2, int y2)
    {
        int dx = x2 - x1;
        int dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static double EaseInOut(double t)
    {
        // ease-in-out: smooth cosine（速度在 t=0/1 处为 0）。
        t = Math.Clamp(t, 0, 1);
        return 0.5 - 0.5 * Math.Cos(Math.PI * t);
    }

    private static void PressModifiers(IReadOnlyList<ushort> modifiers)
    {
        foreach (var vk in modifiers)
        {
            SwgWin32Input.SendKeyboardVirtualKey(vk, isDown: true);
        }
        InputWait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(20));
    }

    private static void ReleaseModifiersReverse(IReadOnlyList<ushort> modifiers)
    {
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            SwgWin32Input.SendKeyboardVirtualKey(modifiers[i], isDown: false);
        }
        InputWait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(20));
    }

    private static IReadOnlyList<ushort> ParseModifiersToVks(IReadOnlyList<string>? modifiers)
    {
        if (modifiers is null || modifiers.Count == 0)
            return Array.Empty<ushort>();

        var list = new List<ushort>(4);
        var seen = new HashSet<ushort>();

        foreach (var raw in modifiers)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var s = raw.Trim();
            if (s.Length > 0 && s.All(ch => ch is '^' or '!' or '+' or '#'))
            {
                foreach (char ch in s)
                {
                    ushort modVk = ch switch
                    {
                        '^' => InputKeyMap.VkCtrl,
                        '!' => InputKeyMap.VkAlt,
                        '+' => InputKeyMap.VkShift,
                        '#' => InputKeyMap.VkWin,
                        _ => throw new ArgumentException($"不支持的修饰键：{raw}。", nameof(modifiers)),
                    };
                    if (seen.Add(modVk))
                        list.Add(modVk);
                }
                continue;
            }

            // 兼容输入为 Ctrl/Alt/Shift/Win 等名字
            string lower = s.ToLowerInvariant();
            ushort? modVk2 = lower switch
            {
                "ctrl" or "control" => InputKeyMap.VkCtrl,
                "alt" or "menu" => InputKeyMap.VkAlt,
                "shift" => InputKeyMap.VkShift,
                "win" or "windows" or "super" or "meta" => InputKeyMap.VkWin,
                _ => null,
            };

            if (modVk2 is null)
                throw new ArgumentException($"不支持的修饰键：{raw}。", nameof(modifiers));

            if (seen.Add(modVk2.Value))
                list.Add(modVk2.Value);
        }

        return list;
    }

    /// <summary>
    /// 从 DTO 的 button 字段解析为 Win32 注入用的枚举。
    /// </summary>
    public static SwgWin32Input.MouseButton ParseMouseButtonOrThrow(string? buttonName)
    {
        if (string.IsNullOrWhiteSpace(buttonName))
            return SwgWin32Input.MouseButton.Left;

        string s = buttonName.Trim().ToLowerInvariant();
        return s switch
        {
            "left" => SwgWin32Input.MouseButton.Left,
            "middle" => SwgWin32Input.MouseButton.Middle,
            "right" => SwgWin32Input.MouseButton.Right,
            "xbutton1" or "x1" => SwgWin32Input.MouseButton.XButton1,
            "xbutton2" or "x2" => SwgWin32Input.MouseButton.XButton2,
            _ => throw new ArgumentException($"不支持的鼠标按钮：{buttonName}。", nameof(buttonName)),
        };
    }
}

