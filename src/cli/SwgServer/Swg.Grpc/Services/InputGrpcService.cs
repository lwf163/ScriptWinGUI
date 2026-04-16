using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Swg.Grpc.Api;
using Swg.Grpc.Input;

namespace Swg.Grpc.Services;

/// <summary>
/// 输入模拟 gRPC 服务实现：提供键盘输入模拟、鼠标操作和光标位置查询。
/// <para>
/// 继承自 <c>InputService.InputServiceBase</c>，由 gRPC 运行时自动注册。
/// 所有 RPC 均通过 <see cref="WindowsGlobalInputGate"/> 进行全局输入序列化，
/// 再经由 <see cref="GrpcRouteRunner"/> 统一异常映射。
/// </para>
/// <para>对应 Proto 定义：<c>swg.input.InputService</c></para>
/// </summary>
public sealed class InputGrpcService : InputService.InputServiceBase
{
    /// <summary>模拟键盘输入完整文本字符串。</summary>
    public override Task<InputOkResponse> TypeText(InputKeyboardTypeTextRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeText(request))));

    /// <summary>模拟键盘输入单个字符。</summary>
    public override Task<InputOkResponse> TypeChar(InputKeyboardTypeCharRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeChar(request))));

    /// <summary>模拟键盘依次按下并释放一组按键。</summary>
    public override Task<InputOkResponse> TypeKeys(InputKeyboardTypeKeysRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeKeys(request))));

    /// <summary>模拟键盘同时按下一组按键（组合键）。</summary>
    public override Task<InputOkResponse> TypeSimultaneously(InputKeyboardTypeSimultaneouslyRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeSimultaneously(request))));

    /// <summary>模拟键盘输入单个按键（按下并释放）。</summary>
    public override Task<InputOkResponse> TypeKey(InputKeyboardTypeKeyRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeKey(request))));

    /// <summary>模拟键盘按下某个按键（不释放）。</summary>
    public override Task<InputOkResponse> Press(InputKeyboardPressRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Press(request))));

    /// <summary>模拟键盘释放某个按键。</summary>
    public override Task<InputOkResponse> Release(InputKeyboardReleaseRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Release(request))));

    /// <summary>模拟键盘输入按键序列。</summary>
    public override Task<InputOkResponse> TypeSequence(InputKeyboardTypeSequenceRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeSequence(request))));

    /// <summary>获取当前鼠标光标位置。</summary>
    public override Task<InputMouseGetPositionResponse> GetCursorPosition(Empty request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.GetCursorPosition())));

    /// <summary>立即设置鼠标光标位置（瞬移）。</summary>
    public override Task<InputOkResponse> SetCursorPosition(InputMouseMoveToRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.SetCursorPosition(request))));

    /// <summary>平滑移动鼠标光标到指定位置（带动画）。</summary>
    public override Task<InputOkResponse> MoveTo(InputMouseMoveToRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.MoveTo(request))));

    /// <summary>将鼠标光标移动指定偏移量（带动画）。</summary>
    public override Task<InputOkResponse> MoveBy(InputMouseMoveByRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.MoveBy(request))));

    /// <summary>获取当前鼠标移动动画配置。</summary>
    public override Task<InputMouseMoveSettingsResponse> GetMoveSettings(Empty request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.GetMoveSettings())));

    /// <summary>更新鼠标移动动画配置。</summary>
    public override Task<InputOkResponse> SetMoveSettings(InputMouseMoveSettingsRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.SetMoveSettings(request))));

    /// <summary>执行鼠标点击操作。</summary>
    public override Task<InputOkResponse> Click(InputMouseClickRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Click(request))));

    /// <summary>按下鼠标按钮（不释放）。</summary>
    public override Task<InputOkResponse> Down(InputMouseDownRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Down(request))));

    /// <summary>释放已按下的鼠标按钮。</summary>
    public override Task<InputOkResponse> Up(InputMouseUpRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Up(request))));

    /// <summary>执行鼠标拖拽到目标坐标。</summary>
    public override Task<InputOkResponse> DragTo(InputMouseDragToRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.DragTo(request))));

    /// <summary>执行鼠标拖拽指定距离。</summary>
    public override Task<InputOkResponse> DragBy(InputMouseDragByDistanceRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.DragBy(request))));

    /// <summary>执行鼠标垂直滚轮操作。</summary>
    public override Task<InputOkResponse> Scroll(InputMouseScrollRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Scroll(request))));

    /// <summary>执行鼠标水平滚轮操作。</summary>
    public override Task<InputOkResponse> HorizontalScroll(InputMouseHorizontalScrollRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.HorizontalScroll(request))));

    /// <summary>阻塞等待指定毫秒数。</summary>
    public override Task<InputOkResponse> Wait(InputWaitRequest request, ServerCallContext context) =>
        WindowsGlobalInputGate.RunAsync(context, () => GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Wait(request))));
}
