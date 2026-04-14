using System.Runtime.InteropServices;

namespace Swg.Win32;

/// <summary>
/// SendInput 相关 Win32（P/Invoke + 结构体/常量）。
/// </summary>
internal static class SendInputNative
{
    internal const uint InputTypeMouse = 0;
    internal const uint InputTypeKeyboard = 1;

    internal const uint MouseEventFMove = 0x0001;
    internal const uint MouseEventFLeftDown = 0x0002;
    internal const uint MouseEventFLeftUp = 0x0004;
    internal const uint MouseEventFRightDown = 0x0008;
    internal const uint MouseEventFRightUp = 0x0010;
    internal const uint MouseEventFMiddleDown = 0x0020;
    internal const uint MouseEventFMiddleUp = 0x0040;
    internal const uint MouseEventFXDown = 0x0080;
    internal const uint MouseEventFXUp = 0x0100;
    internal const uint MouseEventFWheel = 0x0800;
    internal const uint MouseEventFHWHEEL = 0x01000;
    internal const uint MouseEventFAbsolute = 0x8000;
    internal const uint MouseEventFVirtualDesk = 0x4000;

    internal const uint KeyEventFKeyUp = 0x0002;
    internal const uint KeyEventFUnicode = 0x0004;
    internal const uint KeyEventFExtendedKey = 0x0001;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        internal int dx;
        internal int dy;
        internal uint mouseData;
        internal uint dwFlags;
        internal uint time;
        internal nint dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        internal ushort wVk;
        internal ushort wScan;
        internal uint dwFlags;
        internal uint time;
        internal nint dwExtraInfo;
    }

    // 为满足 INPUT 联合布局；本模块当前不使用硬件输入。
    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT
    {
        internal uint uMsg;
        internal ushort wParamL;
        internal ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion
    {
        [FieldOffset(0)]
        internal MOUSEINPUT mouseInput;

        [FieldOffset(0)]
        internal KEYBDINPUT keyboardInput;

        [FieldOffset(0)]
        internal HARDWAREINPUT hardwareInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        internal uint type;
        internal InputUnion U;
    }

    internal static readonly int SizeOfInput = Marshal.SizeOf<INPUT>();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll", SetLastError = false)]
    internal static extern nint GetMessageExtraInfo();
}

