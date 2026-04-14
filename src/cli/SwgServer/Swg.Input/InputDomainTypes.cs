namespace Swg.Input;

/// <summary>虚拟桌面坐标系下的光标位置（领域值类型）。</summary>
public sealed record VirtualDesktopCursorPosition(int X, int Y);

/// <summary>鼠标渐变移动速度参数（领域值类型）。</summary>
public sealed record MouseMoveSettings(double MovePixelsPerMillisecond, double MovePixelsPerStep);
