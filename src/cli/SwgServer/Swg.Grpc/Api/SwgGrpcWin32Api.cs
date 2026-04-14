using Swg.Win32;
using Swg.Grpc.Win32;

namespace Swg.Grpc.Api;

/// <summary>
/// Win32 gRPC 门面：封装 <c>Swg.Win32</c> 能力，与对应 Proto RPC 语义一致。
/// </summary>
public static class SwgGrpcWin32Api
{
    public static WindowHandleResponse FindWindow(FindWindowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.TitleContains) && string.IsNullOrWhiteSpace(request.ClassNameEquals))
            throw new ArgumentException("TitleContains 或 ClassNameEquals 至少一个必填。");

        uint? pid = request.HasProcessId ? request.ProcessId : null;
        string hwnd = SwgWin32Window.FindWindowHandle(
            string.IsNullOrWhiteSpace(request.TitleContains) ? null : request.TitleContains,
            string.IsNullOrWhiteSpace(request.ClassNameEquals) ? null : request.ClassNameEquals,
            pid,
            request.VisibleOnly);
        return new WindowHandleResponse { WindowHandle = hwnd };
    }

    public static WindowHandleResponse GetForegroundWindow() =>
        new() { WindowHandle = SwgWin32Window.GetForegroundWindowHandle() };

    public static OkResponse SetForegroundWindow(ForegroundWindowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Window.SetForegroundWindow(request.WindowHandle);
        return new OkResponse { Ok = true };
    }

    public static WindowInfoResponse GetWindowInfo(WindowInfoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ToWindowInfoResponse(SwgWin32Window.GetWindowInfo(request.WindowHandle));
    }

    public static OkResponse SetWindowPositionResize(WindowPositionResizeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Window.SetWindowPositionResize(request.WindowHandle, request.Left, request.Top, request.Width, request.Height);
        return new OkResponse { Ok = true };
    }

    public static OkResponse SetWindowState(WindowStateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Window.SetWindowState(request.WindowHandle, request.State);
        return new OkResponse { Ok = true };
    }

    public static OkResponse CloseWindow(CloseWindowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Window.CloseWindow(request.WindowHandle);
        return new OkResponse { Ok = true };
    }

    public static WindowProcessIdResponse GetWindowProcessId(WindowProcessIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        uint pid = SwgWin32Window.GetWindowProcessId(request.WindowHandle);
        return new WindowProcessIdResponse { ProcessId = pid };
    }

    public static ChildWindowHandlesResponse EnumChildWindows(EnumChildWindowsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var list = SwgWin32Window.EnumChildWindowHandles(request.ParentWindowHandle);
        var r = new ChildWindowHandlesResponse();
        r.WindowHandles.AddRange(list);
        return r;
    }

    public static WindowHandleResponse FindChildWindow(FindChildWindowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string hwnd = SwgWin32Window.FindChildWindowHandle(
            request.ParentWindowHandle,
            request.TitleContains,
            request.ClassNameEquals,
            request.VisibleOnly);
        return new WindowHandleResponse { WindowHandle = hwnd };
    }

    public static ProcessStartResponse StartProcess(ProcessStartRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        uint pid = SwgWin32Process.StartProcess(request.ExecutablePath, request.Arguments);
        return new ProcessStartResponse { ProcessId = pid };
    }

    public static OkResponse KillProcess(ProcessKillRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Process.KillProcess(request.ProcessId);
        return new OkResponse { Ok = true };
    }

    public static ProcessCurrentIdResponse GetCurrentProcessId() =>
        new() { ProcessId = SwgWin32Process.GetCurrentProcessId() };

    public static ProcessExistsResponse ProcessExists(ProcessExistsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new ProcessExistsResponse { Exists = SwgWin32Process.Exists(request.ProcessId) };
    }

    public static ProcessWaitExitResponse ProcessWaitExit(ProcessWaitExitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        int? timeoutMs = request.HasTimeoutMs ? request.TimeoutMs : null;
        bool exited = SwgWin32Process.WaitExit(request.ProcessId, timeoutMs);
        return new ProcessWaitExitResponse { Exited = exited };
    }

    public static ClipboardTextResponse GetClipboardText() =>
        new() { Text = SwgWin32Clipboard.GetText() };

    public static OkResponse SetClipboardText(ClipboardTextSetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Clipboard.SetText(request.Text);
        return new OkResponse { Ok = true };
    }

    public static ClipboardClearResponse ClearClipboard()
    {
        SwgWin32Clipboard.Clear();
        return new ClipboardClearResponse { Ok = true };
    }

    public static MainScreenResponse GetMainScreen()
    {
        var s = SwgWin32SystemInfo.GetMainScreen();
        return new MainScreenResponse { Width = s.Width, Height = s.Height };
    }

    public static VirtualScreenResponse GetVirtualScreen()
    {
        var v = SwgWin32SystemInfo.GetVirtualScreen();
        return new VirtualScreenResponse { X = v.X, Y = v.Y, Width = v.Width, Height = v.Height };
    }

    public static SystemDpiResponse GetSystemDpi()
    {
        var d = SwgWin32SystemInfo.GetSystemDpi();
        return new SystemDpiResponse { DpiX = d.DpiX, DpiY = d.DpiY };
    }

    public static CursorPositionResponse GetCursorPosition()
    {
        var c = SwgWin32SystemInfo.GetCursorPosition();
        return new CursorPositionResponse { X = c.X, Y = c.Y };
    }

    public static ForegroundWindowInfoResponse GetForegroundWindowInfo()
    {
        var fg = SwgWin32SystemInfo.GetForegroundWindowInfo();
        return new ForegroundWindowInfoResponse { Window = ToWindowInfoResponse(fg.Window) };
    }

    public static WindowMessageSendResponse SendMessage(WindowMessageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        long result = SwgWin32Messages.SendWindowMessage(request.WindowHandle, request.Msg, request.WParam, request.LParam);
        return new WindowMessageSendResponse { Result = result };
    }

    public static WindowMessagePostResponse PostMessage(WindowMessageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Messages.PostWindowMessage(request.WindowHandle, request.Msg, request.WParam, request.LParam);
        return new WindowMessagePostResponse { Ok = true };
    }

    public static WindowHandleAtPointResponse WindowFromPoint(WindowFromPointRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string hwnd = SwgWin32Messages.GetWindowHandleAtPoint(request.X, request.Y);
        return new WindowHandleAtPointResponse { WindowHandle = hwnd };
    }

    public static WindowKeysSendResponse SendKeys(WindowKeysSendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Messages.SendKeysToWindow(request.WindowHandle, request.Keys);
        return new WindowKeysSendResponse { Ok = true };
    }

    public static ControlTextGetResponse GetControlText(ControlTextGetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        int? maxLength = request.HasMaxLength ? request.MaxLength : null;
        string text = SwgWin32Controls.GetControlText(request.ControlHandle, maxLength);
        return new ControlTextGetResponse { Text = text };
    }

    public static WmCommandSendResponse SendWmCommand(WmCommandSendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        long result = SwgWin32Controls.SendWmCommand(request.TargetWindowHandle, request.CommandId, request.NotificationCode, request.SenderHandle);
        return new WmCommandSendResponse { Result = result };
    }

    private static WindowInfoResponse ToWindowInfoResponse(WindowInfo info)
    {
        WindowRect r = info.Rect;
        return new WindowInfoResponse
        {
            WindowHandle = info.WindowHandle,
            Title = info.Title,
            ClassName = info.ClassName,
            ProcessId = info.ProcessId,
            Rect = new WindowRectDto
            {
                Left = r.Left,
                Top = r.Top,
                Right = r.Right,
                Bottom = r.Bottom,
                Width = r.Width,
                Height = r.Height,
            },
        };
    }
}
