import type * as grpc from '@grpc/grpc-js';
import { promisifyUnary } from '../grpc-call.js';
import type {
  ElementInfoResponse,
  ElementRefListResponse,
  ElementRefResponse,
  FlaUiCloseApplicationResponse,
  FlaUiOkResponse,
  FlaUiWaitBusyResponse,
  FlaUiWaitHandleResponse,
  SessionCloseApplicationRequest,
  SessionCreateRequest,
  SessionCreateResponse,
  SessionElementClickRequest,
  SessionElementRequest,
  SessionFindByXPathRequest,
  SessionFindElementRequest,
  SessionIdRequest,
  SessionWaitRequest,
} from '../proto-types.js';

/**
 * FlaUI UI 自动化 gRPC 客户端（Proto 服务 `swg.flaui.AutomationService`）。
 *
 * 提供基于 FlaUI 框架的 Windows UI 自动化能力，包括会话管理、元素查找/遍历、
 * 鼠标点击模拟、焦点控制等功能。所有操作均在会话上下文中执行。
 */
export class FlaUiServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /**
   * 创建 UI 自动化会话，关联到目标应用程序进程。
   * 若目标进程未运行且 `launchIfNotRunning` 为 true，将启动指定可执行文件。
   *
   * @param request 请求参数：
   *   - `automationType`（string，可选）：自动化框架类型（如 `UIA3`、`UIA2`）
   *   - `executablePath`（string，可选）：目标可执行文件路径
   *   - `arguments`（string，可选）：启动参数
   *   - `launchIfNotRunning`（boolean）：进程未运行时是否自动启动
   *   - `processIndex`（number，可选）：同名多实例时的进程索引（0 起始）
   * @returns 包含以下字段：
   *   - `sessionId`（string）：会话唯一标识，后续所有操作需使用此 ID
   *   - `processId`（number）：关联的进程 ID
   *   - `automationType`（string）：实际使用的自动化框架类型
   */
  createSession(request: SessionCreateRequest): Promise<SessionCreateResponse> {
    return promisifyUnary<SessionCreateRequest, SessionCreateResponse>(this.client, 'createSession', request);
  }

  /**
   * 删除指定会话，释放自动化资源（不关闭应用程序）。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：由 `createSession` 返回的会话 ID
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  deleteSession(request: SessionIdRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionIdRequest, FlaUiOkResponse>(this.client, 'deleteSession', request);
  }

  /**
   * 优雅关闭会话关联的应用程序。
   * 尝试发送关闭信号，若失败且 `killIfCloseFails` 为 true，则强制终止进程。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   *   - `killIfCloseFails`（boolean）：优雅关闭失败时是否强制终止
   * @returns 包含以下字段：
   *   - `ok`（boolean）：操作是否成功执行
   *   - `closed`（boolean）：应用程序是否已被关闭（优雅关闭失败且未启用 kill 时为 false）
   */
  closeApplication(request: SessionCloseApplicationRequest): Promise<FlaUiCloseApplicationResponse> {
    return promisifyUnary<SessionCloseApplicationRequest, FlaUiCloseApplicationResponse>(
      this.client,
      'closeApplication',
      request
    );
  }

  /**
   * 强制终止会话关联的应用程序进程。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  killApplication(request: SessionIdRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionIdRequest, FlaUiOkResponse>(this.client, 'killApplication', request);
  }

  /**
   * 等待会话关联的应用程序进入空闲状态（主窗口无待处理消息）。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   *   - `timeout`（可选）：超时设置，含 `hasTimeoutMs`/`timeoutMs`
   * @returns 包含以下字段：
   *   - `ok`（boolean）：操作是否成功执行
   *   - `result`（boolean）：是否在超时前进入空闲状态
   */
  waitWhileBusy(request: SessionWaitRequest): Promise<FlaUiWaitBusyResponse> {
    return promisifyUnary<SessionWaitRequest, FlaUiWaitBusyResponse>(this.client, 'waitWhileBusy', request);
  }

  /**
   * 等待会话关联的应用程序主窗口句柄可用。
   *
   * @param request 请求参数：同 `waitWhileBusy`
   * @returns 包含以下字段：
   *   - `ok`（boolean）：操作是否成功执行
   *   - `result`（boolean）：是否在超时前获取到主窗口句柄
   */
  waitMainHandle(request: SessionWaitRequest): Promise<FlaUiWaitHandleResponse> {
    return promisifyUnary<SessionWaitRequest, FlaUiWaitHandleResponse>(this.client, 'waitMainHandle', request);
  }

  /**
   * 获取会话关联应用程序的主窗口元素引用。
   *
   * @param request 请求参数：同 `waitWhileBusy`
   * @returns 包含 `elementId`（string）：主窗口元素的唯一标识
   */
  getMainWindow(request: SessionWaitRequest): Promise<ElementRefResponse> {
    return promisifyUnary<SessionWaitRequest, ElementRefResponse>(this.client, 'getMainWindow', request);
  }

  /**
   * 获取会话关联应用程序的所有顶级窗口元素引用。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   * @returns 包含 `items`（数组）：顶级窗口元素引用列表，每个含 `elementId`
   */
  getTopLevelWindows(request: SessionIdRequest): Promise<ElementRefListResponse> {
    return promisifyUnary<SessionIdRequest, ElementRefListResponse>(this.client, 'getTopLevelWindows', request);
  }

  /**
   * 获取指定 UI 元素的详细信息（名称、类型、位置、状态等）。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   *   - `elementId`（string，必填）：目标元素 ID
   * @returns 包含以下字段：
   *   - `elementId`（string）：元素唯一标识
   *   - `name`（string）：元素名称
   *   - `automationId`（string）：自动化 ID
   *   - `className`（string）：类名
   *   - `controlType`（string）：控件类型（如 Button、Edit 等）
   *   - `frameworkType`（string）：UI 框架类型
   *   - `isEnabled`（boolean）：是否启用
   *   - `isOffscreen`（boolean）：是否在屏幕外
   *   - `isAvailable`（boolean）：是否可用
   *   - `bounds`：元素边界矩形（x/y/width/height）
   */
  getElementInfo(request: SessionElementRequest): Promise<ElementInfoResponse> {
    return promisifyUnary<SessionElementRequest, ElementInfoResponse>(this.client, 'getElementInfo', request);
  }

  /**
   * 根据条件查找第一个匹配的 UI 元素。
   * 支持按 AutomationId、Name、ClassName、ControlType 等条件组合查找，也可使用 XPath 表达式。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   *   - `find`（必填）：查找条件，含 `rootKind`/`rootElementId`/`scope`/`automationId`/`name`/`className`/`controlType`/`xpath` 等
   * @returns 包含 `elementId`（string）：匹配元素的唯一标识
   */
  findElement(request: SessionFindElementRequest): Promise<ElementRefResponse> {
    return promisifyUnary<SessionFindElementRequest, ElementRefResponse>(this.client, 'findElement', request);
  }

  /**
   * 根据条件查找所有匹配的 UI 元素。
   *
   * @param request 请求参数：同 `findElement`
   * @returns 包含 `items`（数组）：所有匹配元素的 `elementId` 列表
   */
  findAllElements(request: SessionFindElementRequest): Promise<ElementRefListResponse> {
    return promisifyUnary<SessionFindElementRequest, ElementRefListResponse>(this.client, 'findAllElements', request);
  }

  /**
   * 使用 XPath 表达式查找第一个匹配的 UI 元素。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   *   - `find`（必填）：XPath 查找条件，含 `rootKind`/`rootElementId`/`xpath`
   * @returns 包含 `elementId`（string）：匹配元素的唯一标识
   */
  findElementByXPath(request: SessionFindByXPathRequest): Promise<ElementRefResponse> {
    return promisifyUnary<SessionFindByXPathRequest, ElementRefResponse>(this.client, 'findElementByXPath', request);
  }

  /**
   * 使用 XPath 表达式查找所有匹配的 UI 元素。
   *
   * @param request 请求参数：同 `findElementByXPath`
   * @returns 包含 `items`（数组）：所有匹配元素的 `elementId` 列表
   */
  findAllElementsByXPath(request: SessionFindByXPathRequest): Promise<ElementRefListResponse> {
    return promisifyUnary<SessionFindByXPathRequest, ElementRefListResponse>(
      this.client,
      'findAllElementsByXPath',
      request
    );
  }

  /**
   * 获取指定 UI 元素的所有直接子元素。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   *   - `elementId`（string，必填）：父元素 ID
   * @returns 包含 `items`（数组）：子元素的 `elementId` 列表
   */
  getChildren(request: SessionElementRequest): Promise<ElementRefListResponse> {
    return promisifyUnary<SessionElementRequest, ElementRefListResponse>(this.client, 'getChildren', request);
  }

  /**
   * 将输入焦点设置到指定 UI 元素。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   *   - `elementId`（string，必填）：目标元素 ID
   * @returns 包含 `ok`（boolean）：是否成功聚焦
   */
  focus(request: SessionElementRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementRequest, FlaUiOkResponse>(this.client, 'focus', request);
  }

  /**
   * 使用 Win32 SetFocus 设置焦点到指定 UI 元素（绕过 FlaUI 抽象层）。
   *
   * @param request 请求参数：同 `focus`
   * @returns 包含 `ok`（boolean）：是否成功聚焦
   */
  focusNative(request: SessionElementRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementRequest, FlaUiOkResponse>(this.client, 'focusNative', request);
  }

  /**
   * 将指定 UI 元素所在窗口设为前台窗口。
   *
   * @param request 请求参数：同 `focus`
   * @returns 包含 `ok`（boolean）：是否成功设置
   */
  setElementForeground(request: SessionElementRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementRequest, FlaUiOkResponse>(this.client, 'setElementForeground', request);
  }

  /**
   * 对指定 UI 元素执行鼠标左键单击。
   *
   * @param request 请求参数：
   *   - `sessionId`（string，必填）：会话 ID
   *   - `elementId`（string，必填）：目标元素 ID
   *   - `hasClick`（boolean）：是否包含点击选项
   *   - `click`：点击选项，含 `hasMoveMouse`/`moveMouse`（是否先移动鼠标到元素位置）
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  click(request: SessionElementClickRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementClickRequest, FlaUiOkResponse>(this.client, 'click', request);
  }

  /**
   * 对指定 UI 元素执行鼠标左键双击。
   *
   * @param request 请求参数：同 `click`
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  doubleClick(request: SessionElementClickRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementClickRequest, FlaUiOkResponse>(this.client, 'doubleClick', request);
  }

  /**
   * 对指定 UI 元素执行鼠标右键单击。
   *
   * @param request 请求参数：同 `click`
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  rightClick(request: SessionElementClickRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementClickRequest, FlaUiOkResponse>(this.client, 'rightClick', request);
  }

  /**
   * 对指定 UI 元素执行鼠标右键双击。
   *
   * @param request 请求参数：同 `click`
   * @returns 包含 `ok`（boolean）：操作是否成功
   */
  rightDoubleClick(request: SessionElementClickRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementClickRequest, FlaUiOkResponse>(this.client, 'rightDoubleClick', request);
  }
}
