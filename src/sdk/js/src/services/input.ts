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
 * `InputService` 客户端（包 `swg.input`）。
 */
export class InputServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /** 输入整段文本。 */
  typeText(request: InputKeyboardTypeTextRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeTextRequest, InputOkResponse>(this.client, 'typeText', request);
  }
  /** 输入单个字符。 */
  typeChar(request: InputKeyboardTypeCharRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeCharRequest, InputOkResponse>(this.client, 'typeChar', request);
  }
  /** 依次输入多个按键。 */
  typeKeys(request: InputKeyboardTypeKeysRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeKeysRequest, InputOkResponse>(this.client, 'typeKeys', request);
  }
  /** 同时按下多个按键组合。 */
  typeSimultaneously(request: InputKeyboardTypeSimultaneouslyRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeSimultaneouslyRequest, InputOkResponse>(
      this.client,
      'typeSimultaneously',
      request
    );
  }
  /** 输入单个按键。 */
  typeKey(request: InputKeyboardTypeKeyRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeKeyRequest, InputOkResponse>(this.client, 'typeKey', request);
  }
  /** 按下按键。 */
  press(request: InputKeyboardPressRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardPressRequest, InputOkResponse>(this.client, 'press', request);
  }
  /** 释放按键。 */
  release(request: InputKeyboardReleaseRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardReleaseRequest, InputOkResponse>(this.client, 'release', request);
  }
  /** 按序列语法输入。 */
  typeSequence(request: InputKeyboardTypeSequenceRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputKeyboardTypeSequenceRequest, InputOkResponse>(this.client, 'typeSequence', request);
  }
  /** 获取鼠标坐标。 */
  getCursorPosition(request: EmptyRequest = {}): Promise<InputMouseGetPositionResponse> {
    return promisifyUnary<EmptyRequest, InputMouseGetPositionResponse>(this.client, 'getCursorPosition', request);
  }
  /** 设置鼠标坐标。 */
  setCursorPosition(request: InputMouseMoveToRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseMoveToRequest, InputOkResponse>(this.client, 'setCursorPosition', request);
  }
  /** 平滑移动到指定坐标。 */
  moveTo(request: InputMouseMoveToRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseMoveToRequest, InputOkResponse>(this.client, 'moveTo', request);
  }
  /** 按增量移动鼠标。 */
  moveBy(request: InputMouseMoveByRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseMoveByRequest, InputOkResponse>(this.client, 'moveBy', request);
  }
  /** 读取移动参数。 */
  getMoveSettings(request: EmptyRequest = {}): Promise<InputMouseMoveSettingsResponse> {
    return promisifyUnary<EmptyRequest, InputMouseMoveSettingsResponse>(this.client, 'getMoveSettings', request);
  }
  /** 设置移动参数。 */
  setMoveSettings(request: InputMouseMoveSettingsRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseMoveSettingsRequest, InputOkResponse>(this.client, 'setMoveSettings', request);
  }
  /** 点击鼠标。 */
  click(request: InputMouseClickRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseClickRequest, InputOkResponse>(this.client, 'click', request);
  }
  /** 按下鼠标按钮。 */
  down(request: InputMouseDownRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseDownRequest, InputOkResponse>(this.client, 'down', request);
  }
  /** 释放鼠标按钮。 */
  up(request: InputMouseUpRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseUpRequest, InputOkResponse>(this.client, 'up', request);
  }
  /** 拖拽到目标坐标。 */
  dragTo(request: InputMouseDragToRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseDragToRequest, InputOkResponse>(this.client, 'dragTo', request);
  }
  /** 按距离拖拽。 */
  dragBy(request: InputMouseDragByDistanceRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseDragByDistanceRequest, InputOkResponse>(this.client, 'dragBy', request);
  }
  /** 垂直滚动。 */
  scroll(request: InputMouseScrollRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseScrollRequest, InputOkResponse>(this.client, 'scroll', request);
  }
  /** 水平滚动。 */
  horizontalScroll(request: InputMouseHorizontalScrollRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputMouseHorizontalScrollRequest, InputOkResponse>(this.client, 'horizontalScroll', request);
  }
  /** 等待指定时长（毫秒）。 */
  wait(request: InputWaitRequest): Promise<InputOkResponse> {
    return promisifyUnary<InputWaitRequest, InputOkResponse>(this.client, 'wait', request);
  }
}
