using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Swg.Grpc.Api;
using Swg.Grpc.Win32;

namespace Swg.Grpc.Services;

/// <summary>
/// Win32 系统 gRPC 服务实现：提供窗口管理、进程管理、剪贴板操作、系统信息查询、
/// Win32 消息发送、控件文本获取等底层 Windows API 能力。
/// <para>
/// 继承自 <c>Win32Service.Win32ServiceBase</c>，由 gRPC 运行时自动注册。
/// 大部分 RPC 通过 <see cref="GrpcRouteRunner"/> 统一异常映射；
/// SendKeys 操作额外通过 <see cref="WindowsGlobalInputGate"/> 进行全局输入序列化。
/// </para>
/// <para>对应 Proto 定义：<c>swg.win32.Win32Service</c></para>
/// </summary>
public sealed class Win32GrpcService : Win32Service.Win32ServiceBase
{
    /// <summary>根据标题或类名查找顶层窗口句柄。</summary>
    public override Task<WindowHandleResponse> FindWindow(FindWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.FindWindow(request)));

    /// <summary>获取当前前台窗口句柄。</summary>
    public override Task<WindowHandleResponse> GetForegroundWindow(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetForegroundWindow()));

    /// <summary>将指定窗口设为前台窗口。</summary>
    public override Task<OkResponse> SetForegroundWindow(ForegroundWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SetForegroundWindow(request)));

    /// <summary>获取窗口详细信息（标题、类名、进程 ID、位置等）。</summary>
    public override Task<WindowInfoResponse> GetWindowInfo(WindowInfoRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetWindowInfo(request)));

    /// <summary>设置窗口位置和大小。</summary>
    public override Task<OkResponse> SetWindowPositionResize(WindowPositionResizeRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SetWindowPositionResize(request)));

    /// <summary>设置窗口状态（最大化、最小化、还原等）。</summary>
    public override Task<OkResponse> SetWindowState(WindowStateRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SetWindowState(request)));

    /// <summary>关闭指定窗口（发送 WM_CLOSE）。</summary>
    public override Task<OkResponse> CloseWindow(CloseWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.CloseWindow(request)));

    /// <summary>获取窗口所属进程 ID。</summary>
    public override Task<WindowProcessIdResponse> GetWindowProcessId(WindowProcessIdRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetWindowProcessId(request)));

    /// <summary>枚举父窗口的所有子窗口句柄。</summary>
    public override Task<ChildWindowHandlesResponse> EnumChildWindows(EnumChildWindowsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.EnumChildWindows(request)));

    /// <summary>在父窗口的子窗口中查找匹配条件的子窗口。</summary>
    public override Task<WindowHandleResponse> FindChildWindow(FindChildWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.FindChildWindow(request)));

    /// <summary>启动新进程。</summary>
    public override Task<ProcessStartResponse> StartProcess(ProcessStartRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.StartProcess(request)));

    /// <summary>强制终止指定进程。</summary>
    public override Task<OkResponse> KillProcess(ProcessKillRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.KillProcess(request)));

    /// <summary>获取当前（服务端）进程 ID。</summary>
    public override Task<ProcessCurrentIdResponse> GetCurrentProcessId(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetCurrentProcessId()));

    /// <summary>检查指定进程是否仍在运行。</summary>
    public override Task<ProcessExistsResponse> ProcessExists(ProcessExistsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.ProcessExists(request)));

    /// <summary>等待指定进程退出。</summary>
    public override Task<ProcessWaitExitResponse> ProcessWaitExit(ProcessWaitExitRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.ProcessWaitExit(request)));

    /// <summary>获取系统剪贴板文本内容。</summary>
    public override Task<ClipboardTextResponse> GetClipboardText(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetClipboardText()));

    /// <summary>设置系统剪贴板文本内容。</summary>
    public override Task<OkResponse> SetClipboardText(ClipboardTextSetRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SetClipboardText(request)));

    /// <summary>清空系统剪贴板。</summary>
    public override Task<ClipboardClearResponse> ClearClipboard(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.ClearClipboard()));

    /// <summary>获取主显示器分辨率。</summary>
    public override Task<MainScreenResponse> GetMainScreen(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetMainScreen()));

    /// <summary>获取虚拟屏幕（多显示器合并区域）的范围和尺寸。</summary>
    public override Task<VirtualScreenResponse> GetVirtualScreen(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetVirtualScreen()));

    /// <summary>获取系统 DPI 设置。</summary>
    public override Task<SystemDpiResponse> GetSystemDpi(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetSystemDpi()));

    /// <summary>获取当前鼠标光标位置。</summary>
    public override Task<CursorPositionResponse> GetCursorPosition(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetCursorPosition()));

    /// <summary>获取当前前台窗口的详细信息。</summary>
    public override Task<ForegroundWindowInfoResponse> GetForegroundWindowInfo(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetForegroundWindowInfo()));

    /// <summary>向窗口发送消息（SendMessage，同步等待处理完成）。</summary>
    public override Task<WindowMessageSendResponse> SendMessage(WindowMessageRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SendMessage(request)));

    /// <summary>向窗口投递消息（PostMessage，异步投递到消息队列）。</summary>
    public override Task<WindowMessagePostResponse> PostMessage(WindowMessageRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.PostMessage(request)));

    /// <summary>获取屏幕指定坐标处的窗口句柄。</summary>
    public override Task<WindowHandleAtPointResponse> WindowFromPoint(WindowFromPointRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.WindowFromPoint(request)));

    /// <summary>
    /// 向窗口发送按键消息（SendKeys 方式）。
    /// <para>通过 <see cref="WindowsGlobalInputGate"/> 进行全局输入序列化。</para>
    /// </summary>
    public override Task<WindowKeysSendResponse> SendKeys(WindowKeysSendRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SendKeys(request))));

    /// <summary>获取 Win32 控件文本内容。</summary>
    public override Task<ControlTextGetResponse> GetControlText(ControlTextGetRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetControlText(request)));

    /// <summary>向目标窗口发送 WM_COMMAND 消息。</summary>
    public override Task<WmCommandSendResponse> SendWmCommand(WmCommandSendRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SendWmCommand(request)));
}
