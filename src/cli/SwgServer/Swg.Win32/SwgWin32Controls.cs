using System.Text;

namespace Swg.Win32;

/// <summary>
/// 控件文本与 WM_COMMAND 发送相关静态业务函数。
/// </summary>
public static class SwgWin32Controls
{
    public static string GetControlText(string? controlHandle, int? maxLength)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(controlHandle, nameof(controlHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Control handle invalid.");

        int len = Win32Native.GetWindowTextLength(hwnd);
        if (len <= 0)
            return string.Empty;

        int cap = len;
        if (maxLength is not null && maxLength > 0)
            cap = Math.Min(cap, maxLength.Value);

        // Win32 期望缓冲区大小包含结束的 \0。
        var sb = new StringBuilder(cap + 1);
        _ = Win32Native.GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    public static long SendWmCommand(string? targetWindowHandle, uint commandId, uint notificationCode, string? senderHandle)
    {
        nint hwnd = Win32Native.ParseHandleOrThrow(targetWindowHandle, nameof(targetWindowHandle));
        if (!Win32Native.IsWindow(hwnd))
            throw new InvalidOperationException("Target window handle invalid.");

        nint lParam = 0;
        if (!string.IsNullOrWhiteSpace(senderHandle))
            lParam = Win32Native.ParseHandleOrThrow(senderHandle, nameof(senderHandle));

        uint low = commandId & 0xFFFF;
        uint high = notificationCode & 0xFFFF;
        nint wParam = (nint)((high << 16) | low);

        nint r = Win32Native.SendMessage(hwnd, (uint)Win32Native.WmCommand, wParam, lParam);
        return r.ToInt64();
    }
}

