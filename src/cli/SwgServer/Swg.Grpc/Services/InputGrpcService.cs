using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Swg.Grpc.Api;
using Swg.Grpc.Input;

namespace Swg.Grpc.Services;

public sealed class InputGrpcService : InputService.InputServiceBase
{
    public override Task<InputOkResponse> TypeText(InputKeyboardTypeTextRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeText(request)));

    public override Task<InputOkResponse> TypeChar(InputKeyboardTypeCharRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeChar(request)));

    public override Task<InputOkResponse> TypeKeys(InputKeyboardTypeKeysRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeKeys(request)));

    public override Task<InputOkResponse> TypeSimultaneously(InputKeyboardTypeSimultaneouslyRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeSimultaneously(request)));

    public override Task<InputOkResponse> TypeKey(InputKeyboardTypeKeyRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeKey(request)));

    public override Task<InputOkResponse> Press(InputKeyboardPressRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Press(request)));

    public override Task<InputOkResponse> Release(InputKeyboardReleaseRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Release(request)));

    public override Task<InputOkResponse> TypeSequence(InputKeyboardTypeSequenceRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.TypeSequence(request)));

    public override Task<InputMouseGetPositionResponse> GetCursorPosition(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.GetCursorPosition()));

    public override Task<InputOkResponse> SetCursorPosition(InputMouseMoveToRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.SetCursorPosition(request)));

    public override Task<InputOkResponse> MoveTo(InputMouseMoveToRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.MoveTo(request)));

    public override Task<InputOkResponse> MoveBy(InputMouseMoveByRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.MoveBy(request)));

    public override Task<InputMouseMoveSettingsResponse> GetMoveSettings(Empty request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.GetMoveSettings()));

    public override Task<InputOkResponse> SetMoveSettings(InputMouseMoveSettingsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.SetMoveSettings(request)));

    public override Task<InputOkResponse> Click(InputMouseClickRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Click(request)));

    public override Task<InputOkResponse> Down(InputMouseDownRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Down(request)));

    public override Task<InputOkResponse> Up(InputMouseUpRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Up(request)));

    public override Task<InputOkResponse> DragTo(InputMouseDragToRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.DragTo(request)));

    public override Task<InputOkResponse> DragBy(InputMouseDragByDistanceRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.DragBy(request)));

    public override Task<InputOkResponse> Scroll(InputMouseScrollRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Scroll(request)));

    public override Task<InputOkResponse> HorizontalScroll(InputMouseHorizontalScrollRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.HorizontalScroll(request)));

    public override Task<InputOkResponse> Wait(InputWaitRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcInputApi.Wait(request)));
}
