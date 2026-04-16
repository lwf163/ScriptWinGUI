import type * as grpc from '@grpc/grpc-js';
import { promisifyUnary } from '../grpc-call.js';
import type {
  EmptyRequest,
  InputKeyboardPressRequest,
  InputKeyboardReleaseRequest,
  InputKeyboardTypeCharRequest,
  InputKeyboardTypeKeyRequest,
  InputKeyboardTypeKeysRequest,
  InputKeyboardTypeSequenceRequest,
  InputKeyboardTypeSimultaneouslyRequest,
  InputKeyboardTypeTextRequest,
  InputMouseClickRequest,
  InputMouseDownRequest,
  InputMouseDragByDistanceRequest,
  InputMouseDragToRequest,
  InputMouseGetPositionResponse,
  InputMouseHorizontalScrollRequest,
  InputMouseMoveByRequest,
  InputMouseMoveSettingsRequest,
  InputMouseMoveSettingsResponse,
  InputMouseMoveToRequest,
  InputMouseScrollRequest,
  InputMouseUpRequest,
  InputOkResponse,
  InputWaitRequest,
} from '../proto-types.js';

/**
 * 输入模拟 gRPC 客户端（Proto 服务 `swg.input.InputService`）。
 *
 * 提供键盘输入模拟（文本输入、按键操作、组合键）、鼠标操作（移动、点击、拖拽、滚动）
 * 以及光标位置查询等功能。所有方法均为无状态调用，线程安全。
 */
export class InputServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /**
   * 模拟键盘输入完整文本字符串。逐字符输入，支持 Unicode 字符。
   *
   * @param request 请求参数：
   *   - `text`（string，必填）：要输入的文本
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  typeText(request: InputKeyboardTypeTextRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeTextRequest, InputOkResponse>(this.client, 'typeText', request);
  }

  /**
   * 模拟键盘输入单个字符。
   *
   * @param request 请求参数：
   *   - `character`（string，必填）：恰好为 1 个字符
   * @returns 包含 `ok`（boolean）：操作是否成功
   * @throws gRPC `InvalidArgument` — character 为空或长度不为 1
   */
  typeChar(request: InputKeyboardTypeCharRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeCharRequest, InputOkResponse>(this.client, 'typeChar', request);
  }

  /**
   * 模拟键盘依次按下并释放一组按键（按顺序执行 key-down/key-up）。
   *
   * @param request 请求参数：
   *   - `keys`（string[]）：按键名称列表（如 `["Ctrl", "C"]`）
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  typeKeys(request: InputKeyboardTypeKeysRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeKeysRequest, InputOkResponse>(this.client, 'typeKeys', request);
  }

  /**
   * 模拟键盘同时按下一组按键（组合键，如 Ctrl+Alt+Delete）。
   * 所有按键同时按下，然后同时释放。
   *
   * @param request 请求参数：
   *   - `keys`（string[]）：组合键名称列表
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  typeSimultaneously(request: InputKeyboardTypeSimultaneouslyRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeSimultaneouslyRequest, InputOkResponse>(
      this.client,
      'typeSimultaneously',
      request
    );
  }

  /**
   * 模拟键盘输入单个按键（按下并释放）。
   *
   * @param request 请求参数：
   *   - `key`（string，必填）：按键名称（如 `Enter`、`Escape`）
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  typeKey(request: InputKeyboardTypeKeyRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeKeyRequest, InputOkResponse>(this.client, 'typeKey', request);
  }

  /**
   * 模拟键盘按下某个按键（不释放，需配合 `release` 使用）。
   *
   * @param request 请求参数：
   *   - `key`（string，必填）：按键名称
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  press(request: InputKeyboardPressRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardPressRequest, InputOkResponse>(this.client, 'press', request);
  }

  /**
   * 模拟键盘释放某个按键（需配合 `press` 使用）。
   *
   * @param request 请求参数：
   *   - `key`（string，必填）：按键名称
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  release(request: InputKeyboardReleaseRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardReleaseRequest, InputOkResponse>(this.client, 'release', request);
  }

  /**
   * 模拟键盘按顺序输入一组按键序列（支持修饰符表示法，如 `"Ctrl+C"`）。
   *
   * @param request 请求参数：
   *   - `sequence`（string，必填）：按键序列字符串
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  typeSequence(request: InputKeyboardTypeSequenceRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeSequenceRequest, InputOkResponse>(this.client, 'typeSequence', request);
  }

  /**
   * 获取当前鼠标光标位置（屏幕绝对坐标）。
   *
   * @returns 包含以下字段：
   *   - `x`（number）：光标 X 坐标
   *   - `y`（number）：光标 Y 坐标
   */
  getCursorPosition(request: EmptyRequest = {}): Promise<InputMouseGetPositionResponse> {
    return promisifyUnary<EmptyRequest, InputMouseGetPositionResponse>(this.client, 'getCursorPosition', request);
  }

  /**
   * 立即设置鼠标光标位置（瞬移，无动画过程）。
   *
   * @param request 请求参数：
   *   - `x`（number，必填）：目标 X 坐标
   *   - `y`（number，必填）：目标 Y 坐标
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  setCursorPosition(request: InputMouseMoveToRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseMoveToRequest, InputOkResponse>(this.client, 'setCursorPosition', request);
  }

  /**
   * 平滑移动鼠标光标到指定位置（带动画过程）。
   * 移动速度受 `getMoveSettings`/`setMoveSettings` 控制。
   *
   * @param request 请求参数：
   *   - `x`（number，必填）：目标 X 坐标
   *   - `y`（number，必填）：目标 Y 坐标
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  moveTo(request: InputMouseMoveToRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseMoveToRequest, InputOkResponse>(this.client, 'moveTo', request);
  }

  /**
   * 将鼠标光标从当前位置移动指定的偏移量（带动画过程）。
   *
   * @param request 请求参数：
   *   - `deltaX`（number，必填）：X 方向偏移量（正值向右）
   *   - `deltaY`（number，必填）：Y 方向偏移量（正值向下）
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  moveBy(request: InputMouseMoveByRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseMoveByRequest, InputOkResponse>(this.client, 'moveBy', request);
  }

  /**
   * 获取当前鼠标移动动画的配置参数。
   *
   * @returns 包含以下字段：
   *   - `movePixelsPerMillisecond`（number）：每毫秒移动的像素数
   *   - `movePixelsPerStep`（number）：每步移动的像素数
   */
  getMoveSettings(request: EmptyRequest = {}): Promise<InputMouseMoveSettingsResponse> {
    return promisifyUnary<EmptyRequest, InputMouseMoveSettingsResponse>(this.client, 'getMoveSettings', request);
  }

  /**
   * 设置鼠标移动动画的配置参数。
   *
   * @param request 请求参数：
   *   - `hasMovePixelsPerMillisecond`（boolean）：是否更新 movePixelsPerMillisecond
   *   - `movePixelsPerMillisecond`（number）：每毫秒移动的像素数
   *   - `hasMovePixelsPerStep`（boolean）：是否更新 movePixelsPerStep
   *   - `movePixelsPerStep`（number）：每步移动的像素数
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  setMoveSettings(request: InputMouseMoveSettingsRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseMoveSettingsRequest, InputOkResponse>(this.client, 'setMoveSettings', request);
  }

  /**
   * 在指定位置执行鼠标点击操作。
   * 若未指定坐标（`hasX`/`hasY` 为 false），则在当前光标位置点击。
   *
   * @param request 请求参数：
   *   - `hasX` / `x`（number）：点击 X 坐标
   *   - `hasY` / `y`（number）：点击 Y 坐标
   *   - `button`（string，必填）：鼠标按钮（`Left`/`Right`/`Middle`）
   *   - `clickCount`（number）：点击次数（1=单击，2=双击）
   * @returns 包含 `ok`（boolean）：操作是否成功
   * @throws gRPC `InvalidArgument` — button 值无效
   */
  click(request: InputMouseClickRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseClickRequest, InputOkResponse>(this.client, 'click', request);
  }

  /**
   * 在指定位置按下鼠标按钮（不释放，需配合 `up` 使用）。
   *
   * @param request 请求参数：
   *   - `hasX`/`x`/`hasY`/`y`：坐标（同 `click`）
   *   - `button`（string，必填）：鼠标按钮
   *   - `modifiers`（string[]）：修饰键列表（如 `["Ctrl"]`）
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  down(request: InputMouseDownRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseDownRequest, InputOkResponse>(this.client, 'down', request);
  }

  /**
   * 释放已按下的鼠标按钮。
   *
   * @param request 请求参数：
   *   - `button`（string，必填）：鼠标按钮
   *   - `modifiers`（string[]）：修饰键列表
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  up(request: InputMouseUpRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseUpRequest, InputOkResponse>(this.client, 'up', request);
  }

  /**
   * 执行鼠标拖拽操作到指定目标坐标。
   * 从起点按下鼠标，拖动到终点后释放。
   *
   * @param request 请求参数：
   *   - `startX`/`startY`（number，必填）：拖拽起始坐标
   *   - `endX`/`endY`（number，必填）：拖拽目标坐标
   *   - `button`（string，必填）：鼠标按钮
   *   - `modifiers`（string[]）：修饰键列表
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  dragTo(request: InputMouseDragToRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseDragToRequest, InputOkResponse>(this.client, 'dragTo', request);
  }

  /**
   * 执行鼠标拖拽操作到指定偏移距离。
   * 从起点按下鼠标，拖动指定距离后释放。
   *
   * @param request 请求参数：
   *   - `startX`/`startY`（number，必填）：拖拽起始坐标
   *   - `distanceX`/`distanceY`（number，必填）：X/Y 方向的拖拽距离
   *   - `button`（string，必填）：鼠标按钮
   *   - `modifiers`（string[]）：修饰键列表
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  dragBy(request: InputMouseDragByDistanceRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseDragByDistanceRequest, InputOkResponse>(this.client, 'dragBy', request);
  }

  /**
   * 在指定位置执行鼠标垂直滚轮操作。
   *
   * @param request 请求参数：
   *   - `x`/`y`（number，必填）：滚动位置的坐标
   *   - `wheelLines`（number）：滚轮行数（正值向上滚，负值向下滚）
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  scroll(request: InputMouseScrollRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseScrollRequest, InputOkResponse>(this.client, 'scroll', request);
  }

  /**
   * 在指定位置执行鼠标水平滚轮操作。
   *
   * @param request 请求参数：
   *   - `x`/`y`（number，必填）：滚动位置的坐标
   *   - `wheelLines`（number）：水平滚轮行数（正值向右，负值向左）
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  horizontalScroll(request: InputMouseHorizontalScrollRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseHorizontalScrollRequest, InputOkResponse>(this.client, 'horizontalScroll', request);
  }

  /**
   * 阻塞等待指定的毫秒数。
   * 用于在自动化脚本中添加延迟。注意：此操作会阻塞当前 RPC 线程。
   *
   * @param request 请求参数：
   *   - `milliseconds`（number，必填）：等待时间（毫秒），必须 ≥ 0
   * @returns 包含 `ok`（boolean）：操作是否成功
   * @throws gRPC `InvalidArgument` — milliseconds 小于 0
   */
  wait(request: InputWaitRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputWaitRequest, InputOkResponse>(this.client, 'wait', request);
  }
}
