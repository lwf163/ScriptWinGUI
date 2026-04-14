using System;
using System.Runtime.InteropServices;

namespace Swg.Win32;

/// <summary>
/// 输入注入（基于 SendInput），用于供 <c>Swg.Input</c> 实现输入能力。
/// </summary>
public static class SwgWin32Input
{
    /// <summary>
    /// 鼠标按钮枚举。
    /// </summary>
    public enum MouseButton
    {
        Left,
        Middle,
        Right,
        XButton1,
        XButton2,
    }

    /// <summary>
    /// 把“虚拟桌面坐标”（以虚拟桌面左上角为原点）映射到 SendInput 的 absolute 坐标（0..65535）。
    /// </summary>
    public static (int AbsX, int AbsY) ToVirtualDesktopAbsolute(int x, int y, VirtualScreenBounds vd)
    {
        if (vd.Width <= 1 || vd.Height <= 1)
            throw new InvalidOperationException("VirtualScreenBounds.Width/Height 必须大于 1。");

        if (x < 0 || x >= vd.Width)
            throw new ArgumentException("x 越界（虚拟桌面坐标要求从 0 开始）。", nameof(x));
        if (y < 0 || y >= vd.Height)
            throw new ArgumentException("y 越界（虚拟桌面坐标要求从 0 开始）。", nameof(y));

        // absolute 坐标的范围为 0..65535（Windows 要求），并且当使用 MOUSEEVENTF_VIRTUALDESK 时，
        // x/y 会被解释为整个虚拟桌面的归一化坐标。
        //
        // 对齐 FlaUI（src/Input/Mouse.cs -> NormalizeCoordinates）里的整数归一化公式：
        // xAbs = x * 65536 / vScreenWidth + 65536 / (vScreenWidth * 2)
        // yAbs = y * 65536 / vScreenHeight + 65536 / (vScreenHeight * 2)
        // 其中此处 x/y 已经是以虚拟桌面左上角为原点的偏移，因此直接代入 vScreenWidth/vScreenHeight。
        int absX = (int)((x * 65536L) / vd.Width + 65536L / (vd.Width * 2L));
        int absY = (int)((y * 65536L) / vd.Height + 65536L / (vd.Height * 2L));

        absX = Math.Clamp(absX, 0, 65535);
        absY = Math.Clamp(absY, 0, 65535);
        return (absX, absY);
    }

    /// <summary>
    /// 将鼠标移动到指定“虚拟桌面坐标”（不使用渐变；仅用于一次性定位）。
    /// </summary>
    public static void SetCursorPositionVirtualDesktop(int x, int y, VirtualScreenBounds vd)
    {
        if (x < 0 || x >= vd.Width)
            throw new ArgumentException("x 越界（虚拟桌面坐标要求从 0 开始）。", nameof(x));
        if (y < 0 || y >= vd.Height)
            throw new ArgumentException("y 越界（虚拟桌面坐标要求从 0 开始）。", nameof(y));

        // SetCursorPos 使用“屏幕坐标”（以主显示器左上角为原点），因此需要叠加虚拟屏偏移。
        int screenX = vd.X + x;
        int screenY = vd.Y + y;

        bool ok = SendInputNative.SetCursorPos(screenX, screenY);
        if (!ok)
        {
            int err = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"SetCursorPos failed. Win32Error={err}.");
        }

        // FlaUI 的经验：多显示器/不同缩放下偶发落点不准，SetCursorPos 后校验并重试一次。
        var actual1 = SwgWin32SystemInfo.GetCursorPosition();
        if (actual1.X != screenX || actual1.Y != screenY)
        {
            ok = SendInputNative.SetCursorPos(screenX, screenY);
            if (!ok)
            {
                int err = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"SetCursorPos(retry) failed. Win32Error={err}.");
            }

            var actual2 = SwgWin32SystemInfo.GetCursorPosition();
            if (actual2.X != screenX || actual2.Y != screenY)
            {
                int err = Marshal.GetLastWin32Error();
                throw new InvalidOperationException(
                    $"SetCursorPos did not apply. Expected=({screenX},{screenY}), Actual=({actual2.X},{actual2.Y}). Win32Error={err}.");
            }
        }
    }

    /// <summary>
    /// 使用 SendInput 将鼠标移动到指定“虚拟桌面坐标”（absolute + MOUSEEVENTF_VIRTUALDESK）。
    /// </summary>
    public static void SendMouseMoveAbsoluteToVirtualDesktop(int x, int y, VirtualScreenBounds vd)
    {
        var (absX, absY) = ToVirtualDesktopAbsolute(x, y, vd);

        var mi = new SendInputNative.MOUSEINPUT
        {
            dx = absX,
            dy = absY,
            mouseData = 0,
            dwFlags = SendInputNative.MouseEventFMove | SendInputNative.MouseEventFAbsolute | SendInputNative.MouseEventFVirtualDesk,
            time = 0,
            dwExtraInfo = SendInputNative.GetMessageExtraInfo(),
        };

        var input = BuildMouseInput(mi);
        SendInputOrThrow(new[] { input }, expectedCount: 1, actionName: "SendMouseMoveAbsoluteToVirtualDesktop");
    }

    /// <summary>
    /// 使用 SendInput 做相对移动（delta）。
    /// </summary>
    public static void SendMouseMoveRelative(int deltaX, int deltaY)
    {
        var mi = new SendInputNative.MOUSEINPUT
        {
            dx = deltaX,
            dy = deltaY,
            mouseData = 0,
            dwFlags = SendInputNative.MouseEventFMove,
            time = 0,
            dwExtraInfo = SendInputNative.GetMessageExtraInfo(),
        };

        var input = BuildMouseInput(mi);
        SendInputOrThrow(new[] { input }, expectedCount: 1, actionName: "SendMouseMoveRelative");
    }

    /// <summary>
    /// 鼠标按下/抬起。
    /// </summary>
    public static void SendMouseButton(MouseButton button, bool isDown)
    {
        uint flags = button switch
        {
            MouseButton.Left => isDown ? SendInputNative.MouseEventFLeftDown : SendInputNative.MouseEventFLeftUp,
            MouseButton.Middle => isDown ? SendInputNative.MouseEventFMiddleDown : SendInputNative.MouseEventFMiddleUp,
            MouseButton.Right => isDown ? SendInputNative.MouseEventFRightDown : SendInputNative.MouseEventFRightUp,
            MouseButton.XButton1 => isDown ? SendInputNative.MouseEventFXDown : SendInputNative.MouseEventFXUp,
            MouseButton.XButton2 => isDown ? SendInputNative.MouseEventFXDown : SendInputNative.MouseEventFXUp,
            _ => throw new ArgumentOutOfRangeException(nameof(button)),
        };

        uint mouseData = button switch
        {
            MouseButton.XButton1 => 1, // XBUTTON1
            MouseButton.XButton2 => 2, // XBUTTON2
            _ => 0,
        };

        var mi = new SendInputNative.MOUSEINPUT
        {
            dx = 0,
            dy = 0,
            mouseData = mouseData,
            dwFlags = flags,
            time = 0,
            dwExtraInfo = SendInputNative.GetMessageExtraInfo(),
        };

        var input = BuildMouseInput(mi);
        SendInputOrThrow(new[] { input }, expectedCount: 1, actionName: "SendMouseButton");
    }

    /// <summary>
    /// 在当前鼠标位置滚动（垂直滚轮）。
    /// </summary>
    public static void SendMouseWheel(int wheelLines)
    {
        // 语义：120 为 1 单位；Win32 wheelData 需要 signed wheelDelta。
        int wheelDelta = wheelLines * 120;
        uint mouseData = unchecked((uint)wheelDelta);

        var mi = new SendInputNative.MOUSEINPUT
        {
            dx = 0,
            dy = 0,
            mouseData = mouseData,
            dwFlags = SendInputNative.MouseEventFWheel,
            time = 0,
            dwExtraInfo = SendInputNative.GetMessageExtraInfo(),
        };

        var input = BuildMouseInput(mi);
        SendInputOrThrow(new[] { input }, expectedCount: 1, actionName: "SendMouseWheel");
    }

    /// <summary>
    /// 在当前鼠标位置滚动（水平滚轮）。
    /// </summary>
    public static void SendMouseHorizontalWheel(int wheelLines)
    {
        int wheelDelta = wheelLines * 120;
        uint mouseData = unchecked((uint)wheelDelta);

        var mi = new SendInputNative.MOUSEINPUT
        {
            dx = 0,
            dy = 0,
            mouseData = mouseData,
            dwFlags = SendInputNative.MouseEventFHWHEEL,
            time = 0,
            dwExtraInfo = SendInputNative.GetMessageExtraInfo(),
        };

        var input = BuildMouseInput(mi);
        SendInputOrThrow(new[] { input }, expectedCount: 1, actionName: "SendMouseHorizontalWheel");
    }

    /// <summary>
    /// 发送虚拟键（VK）按下/抬起。
    /// </summary>
    public static void SendKeyboardVirtualKey(ushort vk, bool isDown)
    {
        uint dwFlags = 0;
        if (!isDown)
            dwFlags |= SendInputNative.KeyEventFKeyUp;
        if (IsExtendedVirtualKey(vk))
            dwFlags |= SendInputNative.KeyEventFExtendedKey;

        var ki = new SendInputNative.KEYBDINPUT
        {
            wVk = vk,
            wScan = 0,
            dwFlags = dwFlags,
            time = 0,
            dwExtraInfo = SendInputNative.GetMessageExtraInfo(),
        };

        var input = BuildKeyboardInput(ki);
        SendInputOrThrow(new[] { input }, expectedCount: 1, actionName: "SendKeyboardVirtualKey");
    }

    /// <summary>
    /// 发送 Unicode 字符按下/抬起（KEYEVENTF_UNICODE）。
    /// </summary>
    public static void SendKeyboardUnicodeChar(char c, bool isDown)
    {
        uint dwFlags = SendInputNative.KeyEventFUnicode;
        if (!isDown)
            dwFlags |= SendInputNative.KeyEventFKeyUp;

        var ki = new SendInputNative.KEYBDINPUT
        {
            wVk = 0,
            wScan = c,
            dwFlags = dwFlags,
            time = 0,
            dwExtraInfo = SendInputNative.GetMessageExtraInfo(),
        };

        var input = BuildKeyboardInput(ki);
        SendInputOrThrow(new[] { input }, expectedCount: 1, actionName: "SendKeyboardUnicodeChar");
    }

    /// <summary>
    /// 发送 Unicode 字符批量按下/抬起。
    /// 以 2*N 个 INPUT 一次性注入（down/up 成对），用于减少 SendInput 系统调用次数。
    /// </summary>
    public static void SendKeyboardUnicodeCharsBatch(ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
            return;

        var inputs = new SendInputNative.INPUT[chars.Length * 2];

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            ushort scan = c;

            // keydown
            var downKi = new SendInputNative.KEYBDINPUT
            {
                wVk = 0,
                wScan = scan,
                dwFlags = SendInputNative.KeyEventFUnicode,
                time = 0,
                dwExtraInfo = SendInputNative.GetMessageExtraInfo(),
            };
            inputs[i * 2] = BuildKeyboardInput(downKi);

            // keyup
            var upKi = new SendInputNative.KEYBDINPUT
            {
                wVk = 0,
                wScan = scan,
                dwFlags = SendInputNative.KeyEventFUnicode | SendInputNative.KeyEventFKeyUp,
                time = 0,
                dwExtraInfo = SendInputNative.GetMessageExtraInfo(),
            };
            inputs[i * 2 + 1] = BuildKeyboardInput(upKi);
        }

        SendInputOrThrow(inputs, expectedCount: inputs.Length, actionName: "SendKeyboardUnicodeCharsBatch");
    }

    private static bool IsExtendedVirtualKey(ushort vk)
    {
        // 参照 Win32 扩展键规则：导航键等通常需要 KEYEVENTF_EXTENDEDKEY。
        return vk is
            0x2D // INSERT
            or 0x2E // DELETE
            or 0x24 // HOME
            or 0x23 // END
            or 0x21 // PRIOR (PAGEUP)
            or 0x22 // NEXT (PAGEDOWN)
            or 0x25 // LEFT
            or 0x26 // UP
            or 0x27 // RIGHT
            or 0x28 // DOWN
            or 0x5B // LWIN
            or 0x5C // RWIN
            or 0x5D; // APPS
    }

    private static SendInputNative.INPUT BuildMouseInput(SendInputNative.MOUSEINPUT mi)
    {
        var input = new SendInputNative.INPUT
        {
            type = SendInputNative.InputTypeMouse,
            U = new SendInputNative.InputUnion
            {
                mouseInput = mi,
            },
        };
        return input;
    }

    private static SendInputNative.INPUT BuildKeyboardInput(SendInputNative.KEYBDINPUT ki)
    {
        var input = new SendInputNative.INPUT
        {
            type = SendInputNative.InputTypeKeyboard,
            U = new SendInputNative.InputUnion
            {
                keyboardInput = ki,
            },
        };
        return input;
    }

    private static void SendInputOrThrow(SendInputNative.INPUT[] inputs, int expectedCount, string actionName)
    {
        uint sent = SendInputNative.SendInput((uint)inputs.Length, inputs, SendInputNative.SizeOfInput);
        if (sent == 0 || sent != expectedCount)
        {
            int err = Marshal.GetLastWin32Error();
            if (sent == 0)
                throw new InvalidOperationException($"{actionName} failed. Win32Error={err}.");
            throw new InvalidOperationException($"{actionName} partial failed. Expected={expectedCount}, Sent={sent}. Win32Error={err}.");
        }
    }
}

