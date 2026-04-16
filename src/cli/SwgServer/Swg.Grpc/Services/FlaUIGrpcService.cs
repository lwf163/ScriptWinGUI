using Grpc.Core;
using Swg.Grpc.Api;
using Swg.Grpc.Flaui;

namespace Swg.Grpc.Services;

/// <summary>
/// FlaUI UI 自动化 gRPC 服务实现：提供会话管理、元素查找/遍历、鼠标点击模拟、焦点控制。
/// <para>
/// 继承自 <c>AutomationService.AutomationServiceBase</c>，由 gRPC 运行时自动注册。
/// 所有 RPC 均通过 <see cref="GrpcRouteRunner"/> 统一异常映射。
/// 点击类操作额外通过 <see cref="WindowsGlobalInputGate"/> 进行全局输入序列化。
/// </para>
/// <para>对应 Proto 定义：<c>swg.flaui.AutomationService</c></para>
/// </summary>
public sealed class FlaUIGrpcService : AutomationService.AutomationServiceBase
{
    /// <summary>创建自动化会话，关联到目标应用程序进程。</summary>
    public override Task<SessionCreateResponse> CreateSession(SessionCreateRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.CreateSession(request)));

    /// <summary>删除指定会话，释放自动化资源（不关闭应用程序）。</summary>
    public override Task<FlaUiOkResponse> DeleteSession(SessionIdRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.DeleteSession(request)));

    /// <summary>优雅关闭会话关联的应用程序。</summary>
    public override Task<FlaUiCloseApplicationResponse> CloseApplication(SessionCloseApplicationRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.CloseApplication(request)));

    /// <summary>强制终止会话关联的应用程序进程。</summary>
    public override Task<FlaUiOkResponse> KillApplication(SessionIdRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.KillApplication(request)));

    /// <summary>等待应用程序进入空闲状态。</summary>
    public override Task<FlaUiWaitBusyResponse> WaitWhileBusy(SessionWaitRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.WaitWhileBusy(request)));

    /// <summary>等待应用程序主窗口句柄可用。</summary>
    public override Task<FlaUiWaitHandleResponse> WaitMainHandle(SessionWaitRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.WaitMainHandle(request)));

    /// <summary>获取应用程序主窗口元素引用。</summary>
    public override Task<ElementRefResponse> GetMainWindow(SessionWaitRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.GetMainWindow(request)));

    /// <summary>获取应用程序所有顶级窗口元素引用。</summary>
    public override Task<ElementRefListResponse> GetTopLevelWindows(SessionIdRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.GetTopLevelWindows(request)));

    /// <summary>获取指定 UI 元素的详细信息。</summary>
    public override Task<ElementInfoResponse> GetElementInfo(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.GetElementInfo(request)));

    /// <summary>根据条件查找第一个匹配的 UI 元素。</summary>
    public override Task<ElementRefResponse> FindElement(SessionFindElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FindElement(request)));

    /// <summary>根据条件查找所有匹配的 UI 元素。</summary>
    public override Task<ElementRefListResponse> FindAllElements(SessionFindElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FindAllElements(request)));

    /// <summary>使用 XPath 查找第一个匹配的 UI 元素。</summary>
    public override Task<ElementRefResponse> FindElementByXPath(SessionFindByXPathRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FindElementByXPath(request)));

    /// <summary>使用 XPath 查找所有匹配的 UI 元素。</summary>
    public override Task<ElementRefListResponse> FindAllElementsByXPath(SessionFindByXPathRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FindAllElementsByXPath(request)));

    /// <summary>获取指定 UI 元素的所有直接子元素。</summary>
    public override Task<ElementRefListResponse> GetChildren(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.GetChildren(request)));

    /// <summary>将输入焦点设置到指定 UI 元素。</summary>
    public override Task<FlaUiOkResponse> Focus(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.Focus(request)));

    /// <summary>使用 Win32 SetFocus 设置焦点到指定 UI 元素。</summary>
    public override Task<FlaUiOkResponse> FocusNative(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.FocusNative(request)));

    /// <summary>将指定元素所在窗口设为前台窗口。</summary>
    public override Task<FlaUiOkResponse> SetElementForeground(SessionElementRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.SetElementForeground(request)));

    /// <summary>
    /// 对指定 UI 元素执行鼠标左键单击。
    /// <para>通过 <see cref="WindowsGlobalInputGate"/> 进行全局输入序列化。</para>
    /// </summary>
    public override Task<FlaUiOkResponse> Click(SessionElementClickRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.Click(request))));

    /// <summary>
    /// 对指定 UI 元素执行鼠标左键双击。
    /// <para>通过 <see cref="WindowsGlobalInputGate"/> 进行全局输入序列化。</para>
    /// </summary>
    public override Task<FlaUiOkResponse> DoubleClick(SessionElementClickRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.DoubleClick(request))));

    /// <summary>
    /// 对指定 UI 元素执行鼠标右键单击。
    /// <para>通过 <see cref="WindowsGlobalInputGate"/> 进行全局输入序列化。</para>
    /// </summary>
    public override Task<FlaUiOkResponse> RightClick(SessionElementClickRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.RightClick(request))));

    /// <summary>
    /// 对指定 UI 元素执行鼠标右键双击。
    /// <para>通过 <see cref="WindowsGlobalInputGate"/> 进行全局输入序列化。</para>
    /// </summary>
    public override Task<FlaUiOkResponse> RightDoubleClick(SessionElementClickRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFlaUiApi.RightDoubleClick(request))));
}
