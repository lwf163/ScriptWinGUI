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
 * `AutomationService`（FlaUI）客户端（包 `swg.flaui`）。
 */
export class FlaUiServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /** 创建 FlaUI 会话。 */
  createSession(request: SessionCreateRequest): Promise<SessionCreateResponse> {
    return promisifyUnary<SessionCreateRequest, SessionCreateResponse>(this.client, 'createSession', request);
  }
  /** 删除会话。 */
  deleteSession(request: SessionIdRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionIdRequest, FlaUiOkResponse>(this.client, 'deleteSession', request);
  }
  /** 关闭应用进程（可配置失败后强杀）。 */
  closeApplication(request: SessionCloseApplicationRequest): Promise<FlaUiCloseApplicationResponse> {
    return promisifyUnary<SessionCloseApplicationRequest, FlaUiCloseApplicationResponse>(
      this.client,
      'closeApplication',
      request
    );
  }
  /** 强制结束应用。 */
  killApplication(request: SessionIdRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionIdRequest, FlaUiOkResponse>(this.client, 'killApplication', request);
  }
  /** 等待应用空闲。 */
  waitWhileBusy(request: SessionWaitRequest): Promise<FlaUiWaitBusyResponse> {
    return promisifyUnary<SessionWaitRequest, FlaUiWaitBusyResponse>(this.client, 'waitWhileBusy', request);
  }
  /** 等待主窗口句柄可用。 */
  waitMainHandle(request: SessionWaitRequest): Promise<FlaUiWaitHandleResponse> {
    return promisifyUnary<SessionWaitRequest, FlaUiWaitHandleResponse>(this.client, 'waitMainHandle', request);
  }
  /** 获取主窗口元素引用。 */
  getMainWindow(request: SessionWaitRequest): Promise<ElementRefResponse> {
    return promisifyUnary<SessionWaitRequest, ElementRefResponse>(this.client, 'getMainWindow', request);
  }
  /** 获取顶级窗口列表。 */
  getTopLevelWindows(request: SessionIdRequest): Promise<ElementRefListResponse> {
    return promisifyUnary<SessionIdRequest, ElementRefListResponse>(this.client, 'getTopLevelWindows', request);
  }
  /** 获取元素详细信息。 */
  getElementInfo(request: SessionElementRequest): Promise<ElementInfoResponse> {
    return promisifyUnary<SessionElementRequest, ElementInfoResponse>(this.client, 'getElementInfo', request);
  }
  /** 查找单个元素。 */
  findElement(request: SessionFindElementRequest): Promise<ElementRefResponse> {
    return promisifyUnary<SessionFindElementRequest, ElementRefResponse>(this.client, 'findElement', request);
  }
  /** 查找多个元素。 */
  findAllElements(request: SessionFindElementRequest): Promise<ElementRefListResponse> {
    return promisifyUnary<SessionFindElementRequest, ElementRefListResponse>(this.client, 'findAllElements', request);
  }
  /** 通过 XPath 查找单个元素。 */
  findElementByXPath(request: SessionFindByXPathRequest): Promise<ElementRefResponse> {
    return promisifyUnary<SessionFindByXPathRequest, ElementRefResponse>(this.client, 'findElementByXPath', request);
  }
  /** 通过 XPath 查找多个元素。 */
  findAllElementsByXPath(request: SessionFindByXPathRequest): Promise<ElementRefListResponse> {
    return promisifyUnary<SessionFindByXPathRequest, ElementRefListResponse>(
      this.client,
      'findAllElementsByXPath',
      request
    );
  }
  /** 获取元素子节点。 */
  getChildren(request: SessionElementRequest): Promise<ElementRefListResponse> {
    return promisifyUnary<SessionElementRequest, ElementRefListResponse>(this.client, 'getChildren', request);
  }
  /** 设置 FlaUI 焦点。 */
  focus(request: SessionElementRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementRequest, FlaUiOkResponse>(this.client, 'focus', request);
  }
  /** 设置 Native 焦点。 */
  focusNative(request: SessionElementRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementRequest, FlaUiOkResponse>(this.client, 'focusNative', request);
  }
  /** 将元素窗口置前。 */
  setElementForeground(request: SessionElementRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementRequest, FlaUiOkResponse>(this.client, 'setElementForeground', request);
  }
  /** 点击元素。 */
  click(request: SessionElementClickRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementClickRequest, FlaUiOkResponse>(this.client, 'click', request);
  }
  /** 双击元素。 */
  doubleClick(request: SessionElementClickRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementClickRequest, FlaUiOkResponse>(this.client, 'doubleClick', request);
  }
  /** 右键点击元素。 */
  rightClick(request: SessionElementClickRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementClickRequest, FlaUiOkResponse>(this.client, 'rightClick', request);
  }
  /** 右键双击元素。 */
  rightDoubleClick(request: SessionElementClickRequest): Promise<FlaUiOkResponse> {
    return promisifyUnary<SessionElementClickRequest, FlaUiOkResponse>(this.client, 'rightDoubleClick', request);
  }
}
