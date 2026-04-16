using System.Threading;
using Swg.Input;
using Swg.Grpc.Input;

namespace Swg.Grpc.Api;

/// <summary>
/// 输入模拟 gRPC 门面：封装 <c>Swg.Input</c> 能力，与对应 Proto RPC 语义一致。
/// <para>
/// 提供键盘输入模拟（文本输入、按键操作、组合键）、鼠标操作（移动、点击、拖拽、滚动）
/// 以及光标位置查询等功能。所有方法均为无状态静态方法，线程安全。
/// </para>
/// <para>对应 Proto 服务：<c>swg.input.InputService</c></para>
/// <para>
/// 异常映射约定（经由 <see cref="GrpcRouteRunner"/> 统一处理）：
/// <list type="bullet">
///   <item><description><see cref="ArgumentException"/> → <c>StatusCode.InvalidArgument</c></description></item>
///   <item><description><see cref="InvalidOperationException"/> → <c>StatusCode.Unavailable</c></description></item>
///   <item><description>其他异常 → <c>StatusCode.Internal</c></description></item>
/// </list>
/// </para>
/// </summary>
public static class SwgGrpcInputApi
{
    /// <summary>
    /// 模拟键盘输入完整文本字符串。
    /// <para>逐字符输入，支持 Unicode 字符。</para>
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Text</c>（string，必填）：要输入的文本</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse TypeText(InputKeyboardTypeTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeText(request.Text);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 模拟键盘输入单个字符。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Character</c>（string，必填）：恰好为 1 个字符</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Character</c> 为空或长度不为 1</exception>
    public static InputOkResponse TypeChar(InputKeyboardTypeCharRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string s = request.Character ?? throw new ArgumentException("Character 必填。", nameof(request));
        if (s.Length != 1)
            throw new ArgumentException("Character 必须恰好为 1 个字符。");
        InputKeyboard.TypeChar(s[0]);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 模拟键盘依次按下并释放一组按键（按顺序执行 key-down/key-up）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Keys</c>（repeated string）：按键名称列表（如 <c>["Ctrl", "C"]</c>）</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse TypeKeys(InputKeyboardTypeKeysRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeKeys(request.Keys);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 模拟键盘同时按下一组按键（组合键，如 Ctrl+Alt+Delete）。
    /// <para>所有按键同时按下，然后同时释放。</para>
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Keys</c>（repeated string）：组合键名称列表</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse TypeSimultaneously(InputKeyboardTypeSimultaneouslyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeSimultaneously(request.Keys);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 模拟键盘输入单个按键（按下并释放）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Key</c>（string，必填）：按键名称（如 <c>Enter</c>、<c>Escape</c>）</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse TypeKey(InputKeyboardTypeKeyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeKey(request.Key);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 模拟键盘按下某个按键（不释放，需配合 <see cref="Release"/> 使用）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Key</c>（string，必填）：按键名称</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse Press(InputKeyboardPressRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.PressKey(request.Key);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 模拟键盘释放某个按键（需配合 <see cref="Press"/> 使用）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Key</c>（string，必填）：按键名称</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse Release(InputKeyboardReleaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.ReleaseKey(request.Key);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 模拟键盘按顺序输入一组按键序列（支持修饰符表示法，如 <c>"Ctrl+C"</c>）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Sequence</c>（string，必填）：按键序列字符串</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse TypeSequence(InputKeyboardTypeSequenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputKeyboard.TypeSequence(request.Sequence);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 获取当前鼠标光标位置（屏幕绝对坐标）。
    /// </summary>
    /// <returns>
    /// <see cref="InputMouseGetPositionResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>X</c>（int32）：光标 X 坐标</description></item>
    ///   <item><description><c>Y</c>（int32）：光标 Y 坐标</description></item>
    /// </list>
    /// </returns>
    public static InputMouseGetPositionResponse GetCursorPosition()
    {
        var p = InputMouse.GetCursorPosition();
        return new InputMouseGetPositionResponse { X = p.X, Y = p.Y };
    }

    /// <summary>
    /// 立即设置鼠标光标位置（瞬移，无动画过程）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>X</c>（int32，必填）：目标 X 坐标</description></item>
    ///   <item><description><c>Y</c>（int32，必填）：目标 Y 坐标</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse SetCursorPosition(InputMouseMoveToRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.SetCursorPosition(request.X, request.Y);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 平滑移动鼠标光标到指定位置（带动画过程）。
    /// <para>移动速度受 <see cref="GetMoveSettings"/>/<see cref="SetMoveSettings"/> 控制。</para>
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>X</c>（int32，必填）：目标 X 坐标</description></item>
    ///   <item><description><c>Y</c>（int32，必填）：目标 Y 坐标</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse MoveTo(InputMouseMoveToRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.MoveTo(request.X, request.Y);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 将鼠标光标从当前位置移动指定的偏移量（带动画过程）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>DeltaX</c>（int32，必填）：X 方向偏移量（正值向右）</description></item>
    ///   <item><description><c>DeltaY</c>（int32，必填）：Y 方向偏移量（正值向下）</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse MoveBy(InputMouseMoveByRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.MoveBy(request.DeltaX, request.DeltaY);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 获取当前鼠标移动动画的配置参数。
    /// </summary>
    /// <returns>
    /// <see cref="InputMouseMoveSettingsResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>MovePixelsPerMillisecond</c>（double）：每毫秒移动的像素数</description></item>
    ///   <item><description><c>MovePixelsPerStep</c>（double）：每步移动的像素数</description></item>
    /// </list>
    /// </returns>
    public static InputMouseMoveSettingsResponse GetMoveSettings()
    {
        var s = InputMouse.GetMoveSettings();
        return new InputMouseMoveSettingsResponse
        {
            MovePixelsPerMillisecond = s.MovePixelsPerMillisecond,
            MovePixelsPerStep = s.MovePixelsPerStep,
        };
    }

    /// <summary>
    /// 设置鼠标移动动画的配置参数。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>HasMovePixelsPerMillisecond</c>（bool）：是否更新 <c>MovePixelsPerMillisecond</c></description></item>
    ///   <item><description><c>MovePixelsPerMillisecond</c>（double）：每毫秒移动的像素数</description></item>
    ///   <item><description><c>HasMovePixelsPerStep</c>（bool）：是否更新 <c>MovePixelsPerStep</c></description></item>
    ///   <item><description><c>MovePixelsPerStep</c>（double）：每步移动的像素数</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse SetMoveSettings(InputMouseMoveSettingsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        double? mm = request.HasMovePixelsPerMillisecond ? request.MovePixelsPerMillisecond : null;
        double? step = request.HasMovePixelsPerStep ? request.MovePixelsPerStep : null;
        InputMouse.UpdateMoveSettings(mm, step);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 在指定位置执行鼠标点击操作。
    /// <para>若未指定坐标（<c>HasX</c>/<c>HasY</c> 为 false），则在当前光标位置点击。</para>
    /// </summary>
    /// <param name="request">
    /// 点击请求参数：
    /// <list type="bullet">
    ///   <item><description><c>HasX</c>（bool）：是否指定 X 坐标</description></item>
    ///   <item><description><c>X</c>（int32）：点击 X 坐标</description></item>
    ///   <item><description><c>HasY</c>（bool）：是否指定 Y 坐标</description></item>
    ///   <item><description><c>Y</c>（int32）：点击 Y 坐标</description></item>
    ///   <item><description><c>Button</c>（string，必填）：鼠标按钮（<c>Left</c>/<c>Right</c>/<c>Middle</c>）</description></item>
    ///   <item><description><c>ClickCount</c>（int32）：点击次数（1=单击，2=双击）</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Button</c> 值无效</exception>
    public static InputOkResponse Click(InputMouseClickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        int? x = request.HasX ? request.X : null;
        int? y = request.HasY ? request.Y : null;
        InputMouse.Click(x, y, btn, request.ClickCount);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 在指定位置按下鼠标按钮（不释放，需配合 <see cref="Up"/> 使用）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>HasX</c>/<c>X</c>/<c>HasY</c>/<c>Y</c>：坐标（同 <see cref="Click"/>）</description></item>
    ///   <item><description><c>Button</c>（string，必填）：鼠标按钮</description></item>
    ///   <item><description><c>Modifiers</c>（repeated string）：修饰键列表（如 <c>["Ctrl"]</c>）</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse Down(InputMouseDownRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        int? x = request.HasX ? request.X : null;
        int? y = request.HasY ? request.Y : null;
        InputMouse.Press(x, y, btn, request.Modifiers);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 释放已按下的鼠标按钮。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Button</c>（string，必填）：鼠标按钮</description></item>
    ///   <item><description><c>Modifiers</c>（repeated string）：修饰键列表</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse Up(InputMouseUpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        InputMouse.Release(btn, request.Modifiers);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 执行鼠标拖拽操作到指定目标坐标。
    /// <para>从起点按下鼠标，拖动到终点后释放。</para>
    /// </summary>
    /// <param name="request">
    /// 拖拽请求参数：
    /// <list type="bullet">
    ///   <item><description><c>StartX</c>/<c>StartY</c>（int32，必填）：拖拽起始坐标</description></item>
    ///   <item><description><c>EndX</c>/<c>EndY</c>（int32，必填）：拖拽目标坐标</description></item>
    ///   <item><description><c>Button</c>（string，必填）：鼠标按钮</description></item>
    ///   <item><description><c>Modifiers</c>（repeated string）：修饰键列表</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse DragTo(InputMouseDragToRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        InputMouse.DragTo(request.StartX, request.StartY, request.EndX, request.EndY, btn, request.Modifiers);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 执行鼠标拖拽操作到指定偏移距离。
    /// <para>从起点按下鼠标，拖动指定距离后释放。</para>
    /// </summary>
    /// <param name="request">
    /// 拖拽请求参数：
    /// <list type="bullet">
    ///   <item><description><c>StartX</c>/<c>StartY</c>（int32，必填）：拖拽起始坐标</description></item>
    ///   <item><description><c>DistanceX</c>/<c>DistanceY</c>（int32，必填）：X/Y 方向的拖拽距离</description></item>
    ///   <item><description><c>Button</c>（string，必填）：鼠标按钮</description></item>
    ///   <item><description><c>Modifiers</c>（repeated string）：修饰键列表</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse DragBy(InputMouseDragByDistanceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var btn = InputMouse.ParseMouseButtonOrThrow(request.Button);
        InputMouse.DragByDistance(request.StartX, request.StartY, request.DistanceX, request.DistanceY, btn, request.Modifiers);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 在指定位置执行鼠标垂直滚轮操作。
    /// </summary>
    /// <param name="request">
    /// 滚动请求参数：
    /// <list type="bullet">
    ///   <item><description><c>X</c>/<c>Y</c>（int32，必填）：滚动位置的坐标</description></item>
    ///   <item><description><c>WheelLines</c>（int32）：滚轮行数（正值向上滚，负值向下滚）</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse Scroll(InputMouseScrollRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.ScrollAt(request.X, request.Y, request.WheelLines);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 在指定位置执行鼠标水平滚轮操作。
    /// </summary>
    /// <param name="request">
    /// 滚动请求参数：
    /// <list type="bullet">
    ///   <item><description><c>X</c>/<c>Y</c>（int32，必填）：滚动位置的坐标</description></item>
    ///   <item><description><c>WheelLines</c>（int32）：水平滚轮行数（正值向右，负值向左）</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static InputOkResponse HorizontalScroll(InputMouseHorizontalScrollRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        InputMouse.HorizontalScrollAt(request.X, request.Y, request.WheelLines);
        return new InputOkResponse { Ok = true };
    }

    /// <summary>
    /// 阻塞等待指定的毫秒数。
    /// <para>用于在自动化脚本中添加延迟。注意：此操作会阻塞当前 RPC 线程。</para>
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Milliseconds</c>（int32，必填）：等待时间（毫秒），必须 ≥ 0</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="InputOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Milliseconds</c> 小于 0</exception>
    public static InputOkResponse Wait(InputWaitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Milliseconds < 0)
            throw new ArgumentException("Milliseconds 必须 >= 0。", nameof(request));
        Thread.Sleep(request.Milliseconds);
        return new InputOkResponse { Ok = true };
    }
}
