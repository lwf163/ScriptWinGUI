using System.Text;
using System.Runtime.InteropServices;

namespace Swg.Win32;

/// <summary>
/// 剪贴板文本相关静态业务函数（Unicode，避免依赖 Windows Forms）。
/// </summary>
public static class SwgWin32Clipboard
{
    public static string GetText()
    {
        if (!Win32Native.OpenClipboard(0))
            throw new InvalidOperationException("OpenClipboard failed.");

        try
        {
            nint hData = Win32Native.GetClipboardData(Win32Native.CfuUnicodeText);
            if (hData == 0)
                return string.Empty;

            nint ptr = Win32Native.GlobalLock(hData);
            if (ptr == 0)
                throw new InvalidOperationException("GlobalLock failed.");

            try
            {
                return Marshal.PtrToStringUni(ptr) ?? string.Empty;
            }
            finally
            {
                _ = Win32Native.GlobalUnlock(hData);
            }
        }
        finally
        {
            _ = Win32Native.CloseClipboard();
        }
    }

    public static void SetText(string? text)
    {
        text ??= string.Empty;
        byte[] bytes = Encoding.Unicode.GetBytes(text + '\0');

        if (!Win32Native.OpenClipboard(0))
            throw new InvalidOperationException("OpenClipboard failed.");

        nint hGlobal = 0;
        try
        {
            if (!Win32Native.EmptyClipboard())
                throw new InvalidOperationException("EmptyClipboard failed.");

            hGlobal = Win32Native.GlobalAlloc(Win32Native.GmemMoveable, (UIntPtr)bytes.Length);
            if (hGlobal == 0)
                throw new InvalidOperationException("GlobalAlloc failed.");

            nint ptr = Win32Native.GlobalLock(hGlobal);
            if (ptr == 0)
                throw new InvalidOperationException("GlobalLock failed.");

            try
            {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
            }
            finally
            {
                _ = Win32Native.GlobalUnlock(hGlobal);
            }

            nint r = Win32Native.SetClipboardData(Win32Native.CfuUnicodeText, hGlobal);
            if (r == 0)
                throw new InvalidOperationException("SetClipboardData failed.");

            // 成功后 clipboard 拥有内存句柄；不释放 hGlobal。
            hGlobal = 0;
        }
        finally
        {
            _ = Win32Native.CloseClipboard();
            if (hGlobal != 0)
                _ = Win32Native.GlobalFree(hGlobal);
        }
    }

    public static void Clear()
    {
        if (!Win32Native.OpenClipboard(0))
            throw new InvalidOperationException("OpenClipboard failed.");

        try
        {
            if (!Win32Native.EmptyClipboard())
                throw new InvalidOperationException("EmptyClipboard failed.");
        }
        finally
        {
            _ = Win32Native.CloseClipboard();
        }
    }
}

