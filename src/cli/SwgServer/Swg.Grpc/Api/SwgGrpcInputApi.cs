using System.Threading;
using Swg.Input;
using Swg.Grpc.Input;

namespace Swg.Grpc.Api;

/// <summary>
/// 输入模拟 gRPC 门面：封装 <c>Swg.Input</c> 能力，与对应 Proto RPC 语义一致。
/// </summary>
public static class SwgGrpcInputApi
{
    public static InputOkResponse TypeText(InputKeyboardTypeTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeText(request.Text);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse TypeChar(InputKeyboardTypeCharRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string s = request.Character ?? throw new ArgumentException("Character 必填。", nameof(request));
        if (s.Length != 1)
            throw new ArgumentException("Character 必须恰好为 1 个字符。");
        InputKeyboard.TypeChar(s[0]);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse TypeKeys(InputKeyboardTypeKeysRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeKeys(request.Keys);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse TypeSimultaneously(InputKeyboardTypeSimultaneouslyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeSimultaneously(request.Keys);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse TypeKey(InputKeyboardTypeKeyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeKey(request.Key);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse Press(InputKeyboardPressRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.PressKey(request.Key);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse Release(InputKeyboardReleaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.ReleaseKey(request.Key);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse TypeSequence(InputKeyboardTypeSequenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeSequence(request.Sequence);
        return new InputOkResponse { Ok = true };
    }

    public static InputMouseGetPositionResponse GetCursorPosition()
    {
        var p = InputMouse.GetCursorPosition();
        return new InputMouseGetPositionResponse { X = p.X, Y = p.Y };
    }

    public static InputOkResponse SetCursorPosition(InputMouseMoveToRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.SetCursorPosition(request.X, request.Y);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse MoveTo(InputMouseMoveToRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.MoveTo(request.X, request.Y);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse MoveBy(InputMouseMoveByRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.MoveBy(request.DeltaX, request.DeltaY);
        return new InputOkResponse { Ok = true };
    }

    public static InputMouseMoveSettingsResponse GetMoveSettings()
    {
        var s = InputMouse.GetMoveSettings();
        return new InputMouseMoveSettingsResponse
        {
            MovePixelsPerMillisecond = s.MovePixelsPerMillisecond,
            MovePixelsPerStep = s.MovePixelsPerStep,
        };
    }

    public static InputOkResponse SetMoveSettings(InputMouseMoveSettingsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        double? mm = request.HasMovePixelsPerMillisecond ? request.MovePixelsPerMillisecond : null;
        double? step = request.HasMovePixelsPerStep ? request.MovePixelsPerStep : null;
        InputMouse.UpdateMoveSettings(mm, step);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse Click(InputMouseClickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        int? x = request.HasX ? request.X : null;
        int? y = request.HasY ? request.Y : null;
        InputMouse.Click(x, y, btn, request.ClickCount);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse Down(InputMouseDownRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        int? x = request.HasX ? request.X : null;
        int? y = request.HasY ? request.Y : null;
        InputMouse.Press(x, y, btn, request.Modifiers);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse Up(InputMouseUpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        InputMouse.Release(btn, request.Modifiers);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse DragTo(InputMouseDragToRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        InputMouse.DragTo(request.StartX, request.StartY, request.EndX, request.EndY, btn, request.Modifiers);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse DragBy(InputMouseDragByDistanceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        InputMouse.DragByDistance(request.StartX, request.StartY, request.DistanceX, request.DistanceY, btn, request.Modifiers);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse Scroll(InputMouseScrollRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.ScrollAt(request.X, request.Y, request.WheelLines);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse HorizontalScroll(InputMouseHorizontalScrollRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.HorizontalScrollAt(request.X, request.Y, request.WheelLines);
        return new InputOkResponse { Ok = true };
    }

    public static InputOkResponse Wait(InputWaitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Milliseconds < 0)
            throw new ArgumentException("Milliseconds 必须 >= 0。", nameof(request));
        Thread.Sleep(request.Milliseconds);
        return new InputOkResponse { Ok = true };
    }
}
