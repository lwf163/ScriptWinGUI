using Grpc.Core;
using Swg.Grpc.Api;
using Swg.Grpc.Flaui;

namespace Swg.Grpc.Services;

public sealed class FlaUIGrpcService : AutomationService.AutomationServiceBase
{
    public override Task<SessionCreateResponse> CreateSession(SessionCreateRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.CreateSession(request)));

    public override Task<FlaUiOkResponse> DeleteSession(SessionIdRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.DeleteSession(request)));

    public override Task<FlaUiCloseApplicationResponse> CloseApplication(SessionCloseApplicationRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.CloseApplication(request)));

    public override Task<FlaUiOkResponse> KillApplication(SessionIdRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.KillApplication(request)));

    public override Task<FlaUiWaitBusyResponse> WaitWhileBusy(SessionWaitRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.WaitWhileBusy(request)));

    public override Task<FlaUiWaitHandleResponse> WaitMainHandle(SessionWaitRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.WaitMainHandle(request)));

    public override Task<ElementRefResponse> GetMainWindow(SessionWaitRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.GetMainWindow(request)));

    public override Task<ElementRefListResponse> GetTopLevelWindows(SessionIdRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.GetTopLevelWindows(request)));

    public override Task<ElementInfoResponse> GetElementInfo(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.GetElementInfo(request)));

    public override Task<ElementRefResponse> FindElement(SessionFindElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FindElement(request)));

    public override Task<ElementRefListResponse> FindAllElements(SessionFindElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FindAllElements(request)));

    public override Task<ElementRefResponse> FindElementByXPath(SessionFindByXPathRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FindElementByXPath(request)));

    public override Task<ElementRefListResponse> FindAllElementsByXPath(SessionFindByXPathRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FindAllElementsByXPath(request)));

    public override Task<ElementRefListResponse> GetChildren(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.GetChildren(request)));

    public override Task<FlaUiOkResponse> Focus(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.Focus(request)));

    public override Task<FlaUiOkResponse> FocusNative(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FocusNative(request)));

    public override Task<FlaUiOkResponse> SetElementForeground(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.SetElementForeground(request)));

    public override Task<FlaUiOkResponse> Click(SessionElementClickRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.Click(request)));

    public override Task<FlaUiOkResponse> DoubleClick(SessionElementClickRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.DoubleClick(request)));

    public override Task<FlaUiOkResponse> RightClick(SessionElementClickRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.RightClick(request)));

    public override Task<FlaUiOkResponse> RightDoubleClick(SessionElementClickRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.RightDoubleClick(request)));
}
