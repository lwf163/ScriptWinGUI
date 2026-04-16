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
 * Win32 系统 gRPC 客户端（Proto 服务 `swg.win32.Win32Service`）。
 *
 * 提供窗口管理（查找、枚举、操作）、进程管理（启动、终止、等待）、
 * 剪贴板操作、系统信息查询、Win32 消息发送、控件文本获取等底层 Windows API 能力。
 */
export class Win32ServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /**
   * 根据标题或类名查找顶层窗口句柄。
   * `titleContains` 和 `classNameEquals` 至少需要提供一个。
   *
   * @param request 请求参数：
   *   - `titleContains`（string，条件必填）：窗口标题包含的文本
   *   - `classNameEquals`（string，条件必填）：窗口类名精确匹配
   *   - `hasProcessId` / `processId`（number，可选）：进程 ID 过滤
   *   - `visibleOnly`（boolean）：是否只查找可见窗口
   * @returns 包含 `windowHandle`（string）：窗口句柄字符串（十进制数值）
   * @throws gRPC `InvalidArgument` — titleContains 和 classNameEquals 均为空
   */
  findWindow(request: FindWindowRequest): Promise<WindowHandleResponse> {
    return promisifyUnary<FindWindowRequest, WindowHandleResponse>(this.client, 'findWindow', request);
  }

  /**
   * 获取当前前台窗口句柄。
   *
   * @returns 包含 `windowHandle`（string）：前台窗口的句柄
   */
  getForegroundWindow(request: EmptyRequest = {}): Promise<WindowHandleResponse> {
    return promisifyUnary<EmptyRequest, WindowHandleResponse>(this.client, 'getForegroundWindow', request);
  }

  /**
   * 将指定窗口设为前台窗口。
   *
   * @param request 请求参数：
   *   - `windowHandle`（string，必填）：目标窗口句柄
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  setForegroundWindow(request: ForegroundWindowRequest): Promise<OkResponse> {
    return promisifyUnary<ForegroundWindowRequest, OkResponse>(this.client, 'setForegroundWindow', request);
  }

  /**
   * 获取窗口的详细信息（标题、类名、进程 ID、位置等）。
   *
   * @param request 请求参数：
   *   - `windowHandle`（string，必填）：目标窗口句柄
   * @returns 包含以下字段：
   *   - `windowHandle`（string）：窗口句柄
   *   - `title`（string）：窗口标题
   *   - `className`（string）：窗口类名
   *   - `processId`（number）：所属进程 ID
   *   - `rect`：窗口矩形区域，含 `left`/`top`/`right`/`bottom`/`width`/`height`
   */
  getWindowInfo(request: WindowInfoRequest): Promise<WindowInfoResponse> {
    return promisifyUnary<WindowInfoRequest, WindowInfoResponse>(this.client, 'getWindowInfo', request);
  }

  /**
   * 设置窗口的位置和大小。
   *
   * @param request 请求参数：
   *   - `windowHandle`（string，必填）：目标窗口句柄
   *   - `left`/`top`（number）：窗口左上角坐标
   *   - `width`/`height`（number）：窗口宽高
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  setWindowPositionResize(request: WindowPositionResizeRequest): Promise<OkResponse> {
    return promisifyUnary<WindowPositionResizeRequest, OkResponse>(this.client, 'setWindowPositionResize', request);
  }

  /**
   * 设置窗口状态（最大化、最小化、还原等）。
   *
   * @param request 请求参数：
   *   - `windowHandle`（string，必填）：目标窗口句柄
   *   - `state`（string，必填）：窗口状态（如 `Maximize`、`Minimize`、`Restore`）
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  setWindowState(request: WindowStateRequest): Promise<OkResponse> {
    return promisifyUnary<WindowStateRequest, OkResponse>(this.client, 'setWindowState', request);
  }

  /**
   * 关闭指定窗口（发送 WM_CLOSE 消息）。
   *
   * @param request 请求参数：
   *   - `windowHandle`（string，必填）：目标窗口句柄
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  closeWindow(request: CloseWindowRequest): Promise<OkResponse> {
    return promisifyUnary<CloseWindowRequest, OkResponse>(this.client, 'closeWindow', request);
  }

  /**
   * 获取指定窗口所属进程的 ID。
   *
   * @param request 请求参数：
   *   - `windowHandle`（string，必填）：目标窗口句柄
   * @returns 包含 `processId`（number）：进程 ID
   */
  getWindowProcessId(request: WindowProcessIdRequest): Promise<WindowProcessIdResponse> {
    return promisifyUnary<WindowProcessIdRequest, WindowProcessIdResponse>(this.client, 'getWindowProcessId', request);
  }

  /**
   * 枚举指定父窗口的所有子窗口句柄。
   *
   * @param request 请求参数：
   *   - `parentWindowHandle`（string，必填）：父窗口句柄
   * @returns 包含 `windowHandles`（string[]）：子窗口句柄列表
   */
  enumChildWindows(request: EnumChildWindowsRequest): Promise<ChildWindowHandlesResponse> {
    return promisifyUnary<EnumChildWindowsRequest, ChildWindowHandlesResponse>(this.client, 'enumChildWindows', request);
  }

  /**
   * 在指定父窗口的子窗口中查找匹配条件的子窗口。
   *
   * @param request 请求参数：
   *   - `parentWindowHandle`（string，必填）：父窗口句柄
   *   - `titleContains`（string，可选）：子窗口标题包含的文本
   *   - `classNameEquals`（string，可选）：子窗口类名精确匹配
   *   - `visibleOnly`（boolean）：是否只查找可见子窗口
   * @returns 包含 `windowHandle`（string）：匹配子窗口的句柄
   */
  findChildWindow(request: FindChildWindowRequest): Promise<WindowHandleResponse> {
    return promisifyUnary<FindChildWindowRequest, WindowHandleResponse>(this.client, 'findChildWindow', request);
  }

  /**
   * 启动新进程。
   *
   * @param request 请求参数：
   *   - `executablePath`（string，必填）：可执行文件路径
   *   - `arguments`（string，可选）：启动参数
   * @returns 包含 `processId`（number）：新启动进程的 ID
   */
  startProcess(request: ProcessStartRequest): Promise<ProcessStartResponse> {
    return promisifyUnary<ProcessStartRequest, ProcessStartResponse>(this.client, 'startProcess', request);
  }

  /**
   * 强制终止指定进程。
   *
   * @param request 请求参数：
   *   - `processId`（number，必填）：要终止的进程 ID
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  killProcess(request: ProcessKillRequest): Promise<OkResponse> {
    return promisifyUnary<ProcessKillRequest, OkResponse>(this.client, 'killProcess', request);
  }

  /**
   * 获取当前（服务端）进程的 ID。
   *
   * @returns 包含 `processId`（number）：当前进程 ID
   */
  getCurrentProcessId(request: EmptyRequest = {}): Promise<ProcessCurrentIdResponse> {
    return promisifyUnary<EmptyRequest, ProcessCurrentIdResponse>(this.client, 'getCurrentProcessId', request);
  }

  /**
   * 检查指定进程是否仍在运行。
   *
   * @param request 请求参数：
   *   - `processId`（number，必填）：进程 ID
   * @returns 包含 `exists`（boolean）：进程是否存在
   */
  processExists(request: ProcessExistsRequest): Promise<ProcessExistsResponse> {
    return promisifyUnary<ProcessExistsRequest, ProcessExistsResponse>(this.client, 'processExists', request);
  }

  /**
   * 等待指定进程退出。若未指定超时时间，将无限等待。
   *
   * @param request 请求参数：
   *   - `processId`（number，必填）：进程 ID
   *   - `hasTimeoutMs` / `timeoutMs`（number，可选）：超时时间（毫秒）
   * @returns 包含 `exited`（boolean）：进程是否在超时前退出
   */
  processWaitExit(request: ProcessWaitExitRequest): Promise<ProcessWaitExitResponse> {
    return promisifyUnary<ProcessWaitExitRequest, ProcessWaitExitResponse>(this.client, 'processWaitExit', request);
  }

  /**
   * 获取系统剪贴板中的文本内容。
   *
   * @returns 包含 `text`（string）：剪贴板文本内容
   */
  getClipboardText(request: EmptyRequest = {}): Promise<ClipboardTextResponse> {
    return promisifyUnary<EmptyRequest, ClipboardTextResponse>(this.client, 'getClipboardText', request);
  }

  /**
   * 设置系统剪贴板的文本内容。
   *
   * @param request 请求参数：
   *   - `text`（string，必填）：要设置的文本
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  setClipboardText(request: ClipboardTextSetRequest): Promise<OkResponse> {
    return promisifyUnary<ClipboardTextSetRequest, OkResponse>(this.client, 'setClipboardText', request);
  }

  /**
   * 清空系统剪贴板内容。
   *
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  clearClipboard(request: EmptyRequest = {}): Promise<ClipboardClearResponse> {
    return promisifyUnary<EmptyRequest, ClipboardClearResponse>(this.client, 'clearClipboard', request);
  }

  /**
   * 获取主显示器的分辨率。
   *
   * @returns 包含 `width`/`height`（number）：主屏幕分辨率
   */
  getMainScreen(request: EmptyRequest = {}): Promise<MainScreenResponse> {
    return promisifyUnary<EmptyRequest, MainScreenResponse>(this.client, 'getMainScreen', request);
  }

  /**
   * 获取虚拟屏幕（多显示器合并区域）的范围和尺寸。
   *
   * @returns 包含以下字段：
   *   - `x`/`y`（number）：虚拟屏幕左上角坐标
   *   - `width`/`height`（number）：虚拟屏幕尺寸
   */
  getVirtualScreen(request: EmptyRequest = {}): Promise<VirtualScreenResponse> {
    return promisifyUnary<EmptyRequest, VirtualScreenResponse>(this.client, 'getVirtualScreen', request);
  }

  /**
   * 获取系统 DPI 设置。
   *
   * @returns 包含 `dpiX`/`dpiY`（number）：水平和垂直 DPI 值
   */
  getSystemDpi(request: EmptyRequest = {}): Promise<SystemDpiResponse> {
    return promisifyUnary<EmptyRequest, SystemDpiResponse>(this.client, 'getSystemDpi', request);
  }

  /**
   * 获取当前鼠标光标位置（屏幕坐标）。
   *
   * @returns 包含 `x`/`y`（number）：光标坐标
   */
  getCursorPosition(request: EmptyRequest = {}): Promise<CursorPositionResponse> {
    return promisifyUnary<EmptyRequest, CursorPositionResponse>(this.client, 'getCursorPosition', request);
  }

  /**
   * 获取当前前台窗口的详细信息。
   *
   * @returns 包含 `window`：前台窗口信息，字段同 `getWindowInfo`
   */
  getForegroundWindowInfo(request: EmptyRequest = {}): Promise<ForegroundWindowInfoResponse> {
    return promisifyUnary<EmptyRequest, ForegroundWindowInfoResponse>(this.client, 'getForegroundWindowInfo', request);
  }

  /**
   * 向指定窗口发送 Win32 消息（SendMessage，同步等待处理完成）。
   *
   * @param request 消息请求参数：
   *   - `windowHandle`（string，必填）：目标窗口句柄
   *   - `msg`（number，必填）：消息 ID
   *   - `wParam`（string）：WParam 值（字符串形式的数值）
   *   - `lParam`（string）：LParam 值（字符串形式的数值）
   * @returns 包含 `result`（number）：消息处理结果
   */
  sendMessage(request: WindowMessageRequest): Promise<WindowMessageSendResponse> {
    return promisifyUnary<WindowMessageRequest, WindowMessageSendResponse>(this.client, 'sendMessage', request);
  }

  /**
   * 向指定窗口投递 Win32 消息（PostMessage，异步投递到消息队列）。
   *
   * @param request 消息请求参数：同 `sendMessage`
   * @returns 包含 `ok`（boolean）：投递是否成功
   */
  postMessage(request: WindowMessageRequest): Promise<WindowMessagePostResponse> {
    return promisifyUnary<WindowMessageRequest, WindowMessagePostResponse>(this.client, 'postMessage', request);
  }

  /**
   * 获取屏幕指定坐标处的窗口句柄。
   *
   * @param request 请求参数：
   *   - `x`/`y`（number，必填）：屏幕坐标
   * @returns 包含 `windowHandle`（string）：该坐标处的窗口句柄
   */
  windowFromPoint(request: WindowFromPointRequest): Promise<WindowHandleAtPointResponse> {
    return promisifyUnary<WindowFromPointRequest, WindowHandleAtPointResponse>(this.client, 'windowFromPoint', request);
  }

  /**
   * 向指定窗口发送按键消息（SendKeys 方式）。
   *
   * @param request 请求参数：
   *   - `windowHandle`（string，必填）：目标窗口句柄
   *   - `keys`（string[]）：按键名称列表
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  sendKeys(request: WindowKeysSendRequest): Promise<WindowKeysSendResponse> {
    return promisifyUnary<WindowKeysSendRequest, WindowKeysSendResponse>(this.client, 'sendKeys', request);
  }

  /**
   * 获取 Win32 控件的文本内容。
   *
   * @param request 请求参数：
   *   - `controlHandle`（string，必填）：控件句柄
   *   - `hasMaxLength` / `maxLength`（number，可选）：最大获取长度
   * @returns 包含 `text`（string）：控件文本内容
   */
  getControlText(request: ControlTextGetRequest): Promise<ControlTextGetResponse> {
    return promisifyUnary<ControlTextGetRequest, ControlTextGetResponse>(this.client, 'getControlText', request);
  }

  /**
   * 向目标窗口发送 WM_COMMAND 消息。
   *
   * @param request 请求参数：
   *   - `targetWindowHandle`（string，必填）：目标窗口句柄
   *   - `commandId`（number，必填）：命令 ID
   *   - `notificationCode`（number）：通知代码
   *   - `senderHandle`（string）：发送者控件句柄
   * @returns 包含 `result`（number）：消息处理结果
   */
  sendWmCommand(request: WmCommandSendRequest): Promise<WmCommandSendResponse> {
    return promisifyUnary<WmCommandSendRequest, WmCommandSendResponse>(this.client, 'sendWmCommand', request);
  }
}
