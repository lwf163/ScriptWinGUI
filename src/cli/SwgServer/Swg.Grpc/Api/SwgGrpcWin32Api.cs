using Swg.Win32;
using Swg.Grpc.Win32;

namespace Swg.Grpc.Api;

/// <summary>
/// Win32 系统 gRPC 门面：封装 <c>Swg.Win32</c> 能力，与对应 Proto RPC 语义一致。
/// <para>
/// 提供窗口管理（查找、枚举、操作）、进程管理（启动、终止、等待）、
/// 剪贴板操作、系统信息查询、Win32 消息发送、控件文本获取等底层 Windows API 能力。
/// </para>
/// <para>对应 Proto 服务：<c>swg.win32.Win32Service</c></para>
/// <para>
/// 异常映射约定（经由 <see cref="GrpcRouteRunner"/> 统一处理）：
/// <list type="bullet">
///   <item><description><see cref="ArgumentException"/> → <c>StatusCode.InvalidArgument</c></description></item>
///   <item><description><see cref="InvalidOperationException"/> → <c>StatusCode.Unavailable</c></description></item>
///   <item><description>其他异常 → <c>StatusCode.Internal</c></description></item>
/// </list>
/// </para>
/// </summary>
public static class SwgGrpcWin32Api
{
    /// <summary>
    /// 根据标题或类名查找顶层窗口句柄。
    /// <para><c>TitleContains</c> 和 <c>ClassNameEquals</c> 至少需要提供一个。</para>
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>TitleContains</c>（string，条件必填）：窗口标题包含的文本</description></item>
    ///   <item><description><c>ClassNameEquals</c>（string，条件必填）：窗口类名精确匹配</description></item>
    ///   <item><description><c>ProcessId</c>（uint32，可选）：进程 ID 过滤（<c>HasProcessId</c> 为 true 时有效）</description></item>
    ///   <item><description><c>VisibleOnly</c>（bool）：是否只查找可见窗口</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="WindowHandleResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string）：窗口句柄字符串（十进制数值）</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>TitleContains</c> 和 <c>ClassNameEquals</c> 均为空</exception>
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

    /// <summary>
    /// 获取当前前台窗口句柄。
    /// </summary>
    /// <returns>
    /// <see cref="WindowHandleResponse"/>，含前台窗口的 <c>WindowHandle</c>。
    /// </returns>
    public static WindowHandleResponse GetForegroundWindow() =>
        new() { WindowHandle = SwgWin32Window.GetForegroundWindowHandle() };

    /// <summary>
    /// 将指定窗口设为前台窗口。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string，必填）：目标窗口句柄</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="OkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static OkResponse SetForegroundWindow(ForegroundWindowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Window.SetForegroundWindow(request.WindowHandle);
        return new OkResponse { Ok = true };
    }

    /// <summary>
    /// 获取窗口的详细信息（标题、类名、进程 ID、位置等）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string，必填）：目标窗口句柄</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="WindowInfoResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string）：窗口句柄</description></item>
    ///   <item><description><c>Title</c>（string）：窗口标题</description></item>
    ///   <item><description><c>ClassName</c>（string）：窗口类名</description></item>
    ///   <item><description><c>ProcessId</c>（uint32）：所属进程 ID</description></item>
    ///   <item><description><c>Rect</c>（<see cref="WindowRectDto"/>）：窗口矩形区域，含 <c>Left</c>/<c>Top</c>/<c>Right</c>/<c>Bottom</c>/<c>Width</c>/<c>Height</c></description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static WindowInfoResponse GetWindowInfo(WindowInfoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ToWindowInfoResponse(SwgWin32Window.GetWindowInfo(request.WindowHandle));
    }

    /// <summary>
    /// 设置窗口的位置和大小。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string，必填）：目标窗口句柄</description></item>
    ///   <item><description><c>Left</c>/<c>Top</c>（int32）：窗口左上角坐标</description></item>
    ///   <item><description><c>Width</c>/<c>Height</c>（int32）：窗口宽高</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="OkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static OkResponse SetWindowPositionResize(WindowPositionResizeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Window.SetWindowPositionResize(request.WindowHandle, request.Left, request.Top, request.Width, request.Height);
        return new OkResponse { Ok = true };
    }

    /// <summary>
    /// 设置窗口状态（最大化、最小化、还原等）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string，必填）：目标窗口句柄</description></item>
    ///   <item><description><c>State</c>（string，必填）：窗口状态（如 <c>Maximize</c>、<c>Minimize</c>、<c>Restore</c>）</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="OkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static OkResponse SetWindowState(WindowStateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Window.SetWindowState(request.WindowHandle, request.State);
        return new OkResponse { Ok = true };
    }

    /// <summary>
    /// 关闭指定窗口（发送 WM_CLOSE 消息）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string，必填）：目标窗口句柄</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="OkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static OkResponse CloseWindow(CloseWindowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Window.CloseWindow(request.WindowHandle);
        return new OkResponse { Ok = true };
    }

    /// <summary>
    /// 获取指定窗口所属进程的 ID。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string，必填）：目标窗口句柄</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="WindowProcessIdResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>ProcessId</c>（uint32）：进程 ID</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static WindowProcessIdResponse GetWindowProcessId(WindowProcessIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        uint pid = SwgWin32Window.GetWindowProcessId(request.WindowHandle);
        return new WindowProcessIdResponse { ProcessId = pid };
    }

    /// <summary>
    /// 枚举指定父窗口的所有子窗口句柄。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ParentWindowHandle</c>（string，必填）：父窗口句柄</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ChildWindowHandlesResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandles</c>（repeated string）：子窗口句柄列表</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static ChildWindowHandlesResponse EnumChildWindows(EnumChildWindowsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var list = SwgWin32Window.EnumChildWindowHandles(request.ParentWindowHandle);
        var r = new ChildWindowHandlesResponse();
        r.WindowHandles.AddRange(list);
        return r;
    }

    /// <summary>
    /// 在指定父窗口的子窗口中查找匹配条件的子窗口。
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ParentWindowHandle</c>（string，必填）：父窗口句柄</description></item>
    ///   <item><description><c>TitleContains</c>（string，可选）：子窗口标题包含的文本</description></item>
    ///   <item><description><c>ClassNameEquals</c>（string，可选）：子窗口类名精确匹配</description></item>
    ///   <item><description><c>VisibleOnly</c>（bool）：是否只查找可见子窗口</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="WindowHandleResponse"/>，含匹配子窗口的 <c>WindowHandle</c>。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
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

    /// <summary>
    /// 启动新进程。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ExecutablePath</c>（string，必填）：可执行文件路径</description></item>
    ///   <item><description><c>Arguments</c>（string，可选）：启动参数</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ProcessStartResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>ProcessId</c>（uint32）：新启动进程的 ID</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static ProcessStartResponse StartProcess(ProcessStartRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        uint pid = SwgWin32Process.StartProcess(request.ExecutablePath, request.Arguments);
        return new ProcessStartResponse { ProcessId = pid };
    }

    /// <summary>
    /// 强制终止指定进程。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ProcessId</c>（uint32，必填）：要终止的进程 ID</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="OkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static OkResponse KillProcess(ProcessKillRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Process.KillProcess(request.ProcessId);
        return new OkResponse { Ok = true };
    }

    /// <summary>
    /// 获取当前（服务端）进程的 ID。
    /// </summary>
    /// <returns>
    /// <see cref="ProcessCurrentIdResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>ProcessId</c>（uint32）：当前进程 ID</description></item>
    /// </list>
    /// </returns>
    public static ProcessCurrentIdResponse GetCurrentProcessId() =>
        new() { ProcessId = SwgWin32Process.GetCurrentProcessId() };

    /// <summary>
    /// 检查指定进程是否仍在运行。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ProcessId</c>（uint32，必填）：进程 ID</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ProcessExistsResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Exists</c>（bool）：进程是否存在</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static ProcessExistsResponse ProcessExists(ProcessExistsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new ProcessExistsResponse { Exists = SwgWin32Process.Exists(request.ProcessId) };
    }

    /// <summary>
    /// 等待指定进程退出。
    /// <para>若未指定超时时间，将无限等待。</para>
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ProcessId</c>（uint32，必填）：进程 ID</description></item>
    ///   <item><description><c>TimeoutMs</c>（int32，可选）：超时时间（毫秒），<c>HasTimeoutMs</c> 为 true 时有效</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ProcessWaitExitResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Exited</c>（bool）：进程是否在超时前退出</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static ProcessWaitExitResponse ProcessWaitExit(ProcessWaitExitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        int? timeoutMs = request.HasTimeoutMs ? request.TimeoutMs : null;
        bool exited = SwgWin32Process.WaitExit(request.ProcessId, timeoutMs);
        return new ProcessWaitExitResponse { Exited = exited };
    }

    /// <summary>
    /// 获取系统剪贴板中的文本内容。
    /// </summary>
    /// <returns>
    /// <see cref="ClipboardTextResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Text</c>（string）：剪贴板文本内容</description></item>
    /// </list>
    /// </returns>
    public static ClipboardTextResponse GetClipboardText() =>
        new() { Text = SwgWin32Clipboard.GetText() };

    /// <summary>
    /// 设置系统剪贴板的文本内容。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Text</c>（string，必填）：要设置的文本</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="OkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static OkResponse SetClipboardText(ClipboardTextSetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Clipboard.SetText(request.Text);
        return new OkResponse { Ok = true };
    }

    /// <summary>
    /// 清空系统剪贴板内容。
    /// </summary>
    /// <returns><see cref="ClipboardClearResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    public static ClipboardClearResponse ClearClipboard()
    {
        SwgWin32Clipboard.Clear();
        return new ClipboardClearResponse { Ok = true };
    }

    /// <summary>
    /// 获取主显示器的分辨率。
    /// </summary>
    /// <returns>
    /// <see cref="MainScreenResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Width</c>/<c>Height</c>（int32）：主屏幕分辨率</description></item>
    /// </list>
    /// </returns>
    public static MainScreenResponse GetMainScreen()
    {
        var s = SwgWin32SystemInfo.GetMainScreen();
        return new MainScreenResponse { Width = s.Width, Height = s.Height };
    }

    /// <summary>
    /// 获取虚拟屏幕（多显示器合并区域）的范围和尺寸。
    /// </summary>
    /// <returns>
    /// <see cref="VirtualScreenResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>X</c>/<c>Y</c>（int32）：虚拟屏幕左上角坐标</description></item>
    ///   <item><description><c>Width</c>/<c>Height</c>（int32）：虚拟屏幕尺寸</description></item>
    /// </list>
    /// </returns>
    public static VirtualScreenResponse GetVirtualScreen()
    {
        var v = SwgWin32SystemInfo.GetVirtualScreen();
        return new VirtualScreenResponse { X = v.X, Y = v.Y, Width = v.Width, Height = v.Height };
    }

    /// <summary>
    /// 获取系统 DPI 设置。
    /// </summary>
    /// <returns>
    /// <see cref="SystemDpiResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>DpiX</c>/<c>DpiY</c>（int32）：水平和垂直 DPI 值</description></item>
    /// </list>
    /// </returns>
    public static SystemDpiResponse GetSystemDpi()
    {
        var d = SwgWin32SystemInfo.GetSystemDpi();
        return new SystemDpiResponse { DpiX = d.DpiX, DpiY = d.DpiY };
    }

    /// <summary>
    /// 获取当前鼠标光标位置（屏幕坐标）。
    /// </summary>
    /// <returns>
    /// <see cref="CursorPositionResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>X</c>/<c>Y</c>（int32）：光标坐标</description></item>
    /// </list>
    /// </returns>
    public static CursorPositionResponse GetCursorPosition()
    {
        var c = SwgWin32SystemInfo.GetCursorPosition();
        return new CursorPositionResponse { X = c.X, Y = c.Y };
    }

    /// <summary>
    /// 获取当前前台窗口的详细信息。
    /// </summary>
    /// <returns>
    /// <see cref="ForegroundWindowInfoResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Window</c>（<see cref="WindowInfoResponse"/>）：前台窗口信息，字段同 <see cref="GetWindowInfo"/></description></item>
    /// </list>
    /// </returns>
    public static ForegroundWindowInfoResponse GetForegroundWindowInfo()
    {
        var fg = SwgWin32SystemInfo.GetForegroundWindowInfo();
        return new ForegroundWindowInfoResponse { Window = ToWindowInfoResponse(fg.Window) };
    }

    /// <summary>
    /// 向指定窗口发送 Win32 消息（SendMessage，同步等待处理完成）。
    /// </summary>
    /// <param name="request">
    /// 消息请求参数：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string，必填）：目标窗口句柄</description></item>
    ///   <item><description><c>Msg</c>（uint32，必填）：消息 ID</description></item>
    ///   <item><description><c>WParam</c>（string）：WParam 值（字符串形式的数值）</description></item>
    ///   <item><description><c>LParam</c>（string）：LParam 值（字符串形式的数值）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="WindowMessageSendResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Result</c>（int64）：消息处理结果</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static WindowMessageSendResponse SendMessage(WindowMessageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        long result = SwgWin32Messages.SendWindowMessage(request.WindowHandle, request.Msg, request.WParam, request.LParam);
        return new WindowMessageSendResponse { Result = result };
    }

    /// <summary>
    /// 向指定窗口投递 Win32 消息（PostMessage，异步投递到消息队列）。
    /// </summary>
    /// <param name="request">消息请求参数：同 <see cref="SendMessage"/>。</param>
    /// <returns><see cref="WindowMessagePostResponse"/>，<c>Ok</c> 为 true 表示投递成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static WindowMessagePostResponse PostMessage(WindowMessageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Messages.PostWindowMessage(request.WindowHandle, request.Msg, request.WParam, request.LParam);
        return new WindowMessagePostResponse { Ok = true };
    }

    /// <summary>
    /// 获取屏幕指定坐标处的窗口句柄。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>X</c>/<c>Y</c>（int32，必填）：屏幕坐标</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="WindowHandleAtPointResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string）：该坐标处的窗口句柄</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static WindowHandleAtPointResponse WindowFromPoint(WindowFromPointRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string hwnd = SwgWin32Messages.GetWindowHandleAtPoint(request.X, request.Y);
        return new WindowHandleAtPointResponse { WindowHandle = hwnd };
    }

    /// <summary>
    /// 向指定窗口发送按键消息（SendKeys 方式）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string，必填）：目标窗口句柄</description></item>
    ///   <item><description><c>Keys</c>（repeated string）：按键名称列表</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="WindowKeysSendResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static WindowKeysSendResponse SendKeys(WindowKeysSendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgWin32Messages.SendKeysToWindow(request.WindowHandle, request.Keys);
        return new WindowKeysSendResponse { Ok = true };
    }

    /// <summary>
    /// 获取 Win32 控件的文本内容。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ControlHandle</c>（string，必填）：控件句柄</description></item>
    ///   <item><description><c>MaxLength</c>（int32，可选）：最大获取长度（<c>HasMaxLength</c> 为 true 时有效）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ControlTextGetResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Text</c>（string）：控件文本内容</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static ControlTextGetResponse GetControlText(ControlTextGetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        int? maxLength = request.HasMaxLength ? request.MaxLength : null;
        string text = SwgWin32Controls.GetControlText(request.ControlHandle, maxLength);
        return new ControlTextGetResponse { Text = text };
    }

    /// <summary>
    /// 向目标窗口发送 WM_COMMAND 消息。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>TargetWindowHandle</c>（string，必填）：目标窗口句柄</description></item>
    ///   <item><description><c>CommandId</c>（uint32，必填）：命令 ID</description></item>
    ///   <item><description><c>NotificationCode</c>（uint32）：通知代码</description></item>
    ///   <item><description><c>SenderHandle</c>（string）：发送者控件句柄</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="WmCommandSendResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Result</c>（int64）：消息处理结果</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
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
