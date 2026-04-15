import type * as grpc from '@grpc/grpc-js';
import { promisifyUnary } from '../grpc-call.js';
import type {
  ChildWindowHandlesResponse,
  ClipboardClearResponse,
  ClipboardTextResponse,
  ClipboardTextSetRequest,
  CloseWindowRequest,
  ControlTextGetRequest,
  ControlTextGetResponse,
  CursorPositionResponse,
  EmptyRequest,
  EnumChildWindowsRequest,
  FindChildWindowRequest,
  FindWindowRequest,
  ForegroundWindowInfoResponse,
  ForegroundWindowRequest,
  MainScreenResponse,
  OkResponse,
  ProcessCurrentIdResponse,
  ProcessExistsRequest,
  ProcessExistsResponse,
  ProcessKillRequest,
  ProcessStartRequest,
  ProcessStartResponse,
  ProcessWaitExitRequest,
  ProcessWaitExitResponse,
  SystemDpiResponse,
  VirtualScreenResponse,
  WindowFromPointRequest,
  WindowHandleAtPointResponse,
  WindowHandleResponse,
  WindowInfoRequest,
  WindowInfoResponse,
  WindowKeysSendRequest,
  WindowKeysSendResponse,
  WindowMessagePostResponse,
  WindowMessageRequest,
  WindowMessageSendResponse,
  WindowPositionResizeRequest,
  WindowProcessIdRequest,
  WindowProcessIdResponse,
  WindowStateRequest,
  WmCommandSendRequest,
  WmCommandSendResponse,
} from '../proto-types.js';

/**
 * `Win32Service` 客户端（包 `swg.win32`）。
 *
 * RPC 列表见仓库 `Swg.Grpc/Protos/win32.proto`。
 */
export class Win32ServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /** 按条件查找窗口句柄。 */
  findWindow(request: FindWindowRequest): Promise<WindowHandleResponse> {
    return promisifyUnary<FindWindowRequest, WindowHandleResponse>(this.client, 'findWindow', request);
  }
  /** 获取当前前台窗口。 */
  getForegroundWindow(request: EmptyRequest = {}): Promise<WindowHandleResponse> {
    return promisifyUnary<EmptyRequest, WindowHandleResponse>(this.client, 'getForegroundWindow', request);
  }
  /** 将目标窗口设为前台窗口。 */
  setForegroundWindow(request: ForegroundWindowRequest): Promise<OkResponse> {
    return promisifyUnary<ForegroundWindowRequest, OkResponse>(this.client, 'setForegroundWindow', request);
  }
  /** 获取窗口信息。 */
  getWindowInfo(request: WindowInfoRequest): Promise<WindowInfoResponse> {
    return promisifyUnary<WindowInfoRequest, WindowInfoResponse>(this.client, 'getWindowInfo', request);
  }
  /** 设置窗口位置和大小。 */
  setWindowPositionResize(request: WindowPositionResizeRequest): Promise<OkResponse> {
    return promisifyUnary<WindowPositionResizeRequest, OkResponse>(this.client, 'setWindowPositionResize', request);
  }
  /** 设置窗口状态（最小化/最大化等）。 */
  setWindowState(request: WindowStateRequest): Promise<OkResponse> {
    return promisifyUnary<WindowStateRequest, OkResponse>(this.client, 'setWindowState', request);
  }
  /** 关闭窗口。 */
  closeWindow(request: CloseWindowRequest): Promise<OkResponse> {
    return promisifyUnary<CloseWindowRequest, OkResponse>(this.client, 'closeWindow', request);
  }
  /** 获取窗口所属进程 ID。 */
  getWindowProcessId(request: WindowProcessIdRequest): Promise<WindowProcessIdResponse> {
    return promisifyUnary<WindowProcessIdRequest, WindowProcessIdResponse>(this.client, 'getWindowProcessId', request);
  }
  /** 枚举子窗口句柄。 */
  enumChildWindows(request: EnumChildWindowsRequest): Promise<ChildWindowHandlesResponse> {
    return promisifyUnary<EnumChildWindowsRequest, ChildWindowHandlesResponse>(this.client, 'enumChildWindows', request);
  }
  /** 查找子窗口句柄。 */
  findChildWindow(request: FindChildWindowRequest): Promise<WindowHandleResponse> {
    return promisifyUnary<FindChildWindowRequest, WindowHandleResponse>(this.client, 'findChildWindow', request);
  }
  /** 启动进程。 */
  startProcess(request: ProcessStartRequest): Promise<ProcessStartResponse> {
    return promisifyUnary<ProcessStartRequest, ProcessStartResponse>(this.client, 'startProcess', request);
  }
  /** 结束进程。 */
  killProcess(request: ProcessKillRequest): Promise<OkResponse> {
    return promisifyUnary<ProcessKillRequest, OkResponse>(this.client, 'killProcess', request);
  }
  /** 获取当前进程 ID。 */
  getCurrentProcessId(request: EmptyRequest = {}): Promise<ProcessCurrentIdResponse> {
    return promisifyUnary<EmptyRequest, ProcessCurrentIdResponse>(this.client, 'getCurrentProcessId', request);
  }
  /** 判断进程是否存在。 */
  processExists(request: ProcessExistsRequest): Promise<ProcessExistsResponse> {
    return promisifyUnary<ProcessExistsRequest, ProcessExistsResponse>(this.client, 'processExists', request);
  }
  /** 等待进程退出。 */
  processWaitExit(request: ProcessWaitExitRequest): Promise<ProcessWaitExitResponse> {
    return promisifyUnary<ProcessWaitExitRequest, ProcessWaitExitResponse>(this.client, 'processWaitExit', request);
  }
  /** 读取剪贴板文本。 */
  getClipboardText(request: EmptyRequest = {}): Promise<ClipboardTextResponse> {
    return promisifyUnary<EmptyRequest, ClipboardTextResponse>(this.client, 'getClipboardText', request);
  }
  /** 写入剪贴板文本。 */
  setClipboardText(request: ClipboardTextSetRequest): Promise<OkResponse> {
    return promisifyUnary<ClipboardTextSetRequest, OkResponse>(this.client, 'setClipboardText', request);
  }
  /** 清空剪贴板。 */
  clearClipboard(request: EmptyRequest = {}): Promise<ClipboardClearResponse> {
    return promisifyUnary<EmptyRequest, ClipboardClearResponse>(this.client, 'clearClipboard', request);
  }
  /** 获取主屏幕尺寸。 */
  getMainScreen(request: EmptyRequest = {}): Promise<MainScreenResponse> {
    return promisifyUnary<EmptyRequest, MainScreenResponse>(this.client, 'getMainScreen', request);
  }
  /** 获取虚拟屏幕范围。 */
  getVirtualScreen(request: EmptyRequest = {}): Promise<VirtualScreenResponse> {
    return promisifyUnary<EmptyRequest, VirtualScreenResponse>(this.client, 'getVirtualScreen', request);
  }
  /** 获取系统 DPI。 */
  getSystemDpi(request: EmptyRequest = {}): Promise<SystemDpiResponse> {
    return promisifyUnary<EmptyRequest, SystemDpiResponse>(this.client, 'getSystemDpi', request);
  }
  /** 获取当前光标位置。 */
  getCursorPosition(request: EmptyRequest = {}): Promise<CursorPositionResponse> {
    return promisifyUnary<EmptyRequest, CursorPositionResponse>(this.client, 'getCursorPosition', request);
  }
  /** 获取前台窗口详细信息。 */
  getForegroundWindowInfo(request: EmptyRequest = {}): Promise<ForegroundWindowInfoResponse> {
    return promisifyUnary<EmptyRequest, ForegroundWindowInfoResponse>(this.client, 'getForegroundWindowInfo', request);
  }
  /** 发送窗口消息（SendMessage）。 */
  sendMessage(request: WindowMessageRequest): Promise<WindowMessageSendResponse> {
    return promisifyUnary<WindowMessageRequest, WindowMessageSendResponse>(this.client, 'sendMessage', request);
  }
  /** 投递窗口消息（PostMessage）。 */
  postMessage(request: WindowMessageRequest): Promise<WindowMessagePostResponse> {
    return promisifyUnary<WindowMessageRequest, WindowMessagePostResponse>(this.client, 'postMessage', request);
  }
  /** 根据屏幕坐标获取窗口句柄。 */
  windowFromPoint(request: WindowFromPointRequest): Promise<WindowHandleAtPointResponse> {
    return promisifyUnary<WindowFromPointRequest, WindowHandleAtPointResponse>(this.client, 'windowFromPoint', request);
  }
  /** 向窗口发送按键序列。 */
  sendKeys(request: WindowKeysSendRequest): Promise<WindowKeysSendResponse> {
    return promisifyUnary<WindowKeysSendRequest, WindowKeysSendResponse>(this.client, 'sendKeys', request);
  }
  /** 获取控件文本。 */
  getControlText(request: ControlTextGetRequest): Promise<ControlTextGetResponse> {
    return promisifyUnary<ControlTextGetRequest, ControlTextGetResponse>(this.client, 'getControlText', request);
  }
  /** 发送 WM_COMMAND。 */
  sendWmCommand(request: WmCommandSendRequest): Promise<WmCommandSendResponse> {
    return promisifyUnary<WmCommandSendRequest, WmCommandSendResponse>(this.client, 'sendWmCommand', request);
  }
}
