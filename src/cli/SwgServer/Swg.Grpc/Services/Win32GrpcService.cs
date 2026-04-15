using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Swg.Grpc.Api;
using Swg.Grpc.Win32;

namespace Swg.Grpc.Services;

public sealed class Win32GrpcService : Win32Service.Win32ServiceBase
{
    public override Task<WindowHandleResponse> FindWindow(FindWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.FindWindow(request)));

    public override Task<WindowHandleResponse> GetForegroundWindow(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetForegroundWindow()));

    public override Task<OkResponse> SetForegroundWindow(ForegroundWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SetForegroundWindow(request)));

    public override Task<WindowInfoResponse> GetWindowInfo(WindowInfoRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetWindowInfo(request)));

    public override Task<OkResponse> SetWindowPositionResize(WindowPositionResizeRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SetWindowPositionResize(request)));

    public override Task<OkResponse> SetWindowState(WindowStateRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SetWindowState(request)));

    public override Task<OkResponse> CloseWindow(CloseWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.CloseWindow(request)));

    public override Task<WindowProcessIdResponse> GetWindowProcessId(WindowProcessIdRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetWindowProcessId(request)));

    public override Task<ChildWindowHandlesResponse> EnumChildWindows(EnumChildWindowsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.EnumChildWindows(request)));

    public override Task<WindowHandleResponse> FindChildWindow(FindChildWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.FindChildWindow(request)));

    public override Task<ProcessStartResponse> StartProcess(ProcessStartRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.StartProcess(request)));

    public override Task<OkResponse> KillProcess(ProcessKillRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.KillProcess(request)));

    public override Task<ProcessCurrentIdResponse> GetCurrentProcessId(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetCurrentProcessId()));

    public override Task<ProcessExistsResponse> ProcessExists(ProcessExistsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.ProcessExists(request)));

    public override Task<ProcessWaitExitResponse> ProcessWaitExit(ProcessWaitExitRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.ProcessWaitExit(request)));

    public override Task<ClipboardTextResponse> GetClipboardText(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetClipboardText()));

    public override Task<OkResponse> SetClipboardText(ClipboardTextSetRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SetClipboardText(request)));

    public override Task<ClipboardClearResponse> ClearClipboard(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.ClearClipboard()));

    public override Task<MainScreenResponse> GetMainScreen(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetMainScreen()));

    public override Task<VirtualScreenResponse> GetVirtualScreen(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetVirtualScreen()));

    public override Task<SystemDpiResponse> GetSystemDpi(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetSystemDpi()));

    public override Task<CursorPositionResponse> GetCursorPosition(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetCursorPosition()));

    public override Task<ForegroundWindowInfoResponse> GetForegroundWindowInfo(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetForegroundWindowInfo()));

    public override Task<WindowMessageSendResponse> SendMessage(WindowMessageRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SendMessage(request)));

    public override Task<WindowMessagePostResponse> PostMessage(WindowMessageRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.PostMessage(request)));

    public override Task<WindowHandleAtPointResponse> WindowFromPoint(WindowFromPointRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.WindowFromPoint(request)));

    public override Task<WindowKeysSendResponse> SendKeys(WindowKeysSendRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SendKeys(request))));

    public override Task<ControlTextGetResponse> GetControlText(ControlTextGetRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.GetControlText(request)));

    public override Task<WmCommandSendResponse> SendWmCommand(WmCommandSendRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcWin32Api.SendWmCommand(request)));
}
