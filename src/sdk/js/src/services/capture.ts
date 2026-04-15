import type * as grpc from '@grpc/grpc-js';
import { promisifyUnary, serverStreamingToAsyncIterable } from '../grpc-call.js';
import type {
  CaptureCreateListenWindowRequest,
  CaptureCreateListenWindowResponse,
  CaptureHistoryQueryRequest,
  CaptureHistoryQueryResponse,
  CaptureNotificationEvent,
  CaptureStopListenWindowRequest,
  CaptureStopListenWindowResponse,
  EmptyRequest,
  TrafficChunk,
} from '../proto-types.js';

/**
 * `CaptureService` 客户端（包 `swg.capture`）。
 */
export class CaptureServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /** 创建监听窗口并返回窗口 ID 与落盘信息。 */
  createListenWindow(request: CaptureCreateListenWindowRequest): Promise<CaptureCreateListenWindowResponse> {
    return promisifyUnary<CaptureCreateListenWindowRequest, CaptureCreateListenWindowResponse>(
      this.client,
      'createListenWindow',
      request
    );
  }

  /** 停止监听窗口。 */
  stopListenWindow(request: CaptureStopListenWindowRequest): Promise<CaptureStopListenWindowResponse> {
    return promisifyUnary<CaptureStopListenWindowRequest, CaptureStopListenWindowResponse>(
      this.client,
      'stopListenWindow',
      request
    );
  }

  /** 查询抓包历史记录。 */
  queryHistory(request: CaptureHistoryQueryRequest): Promise<CaptureHistoryQueryResponse> {
    return promisifyUnary<CaptureHistoryQueryRequest, CaptureHistoryQueryResponse>(
      this.client,
      'queryHistory',
      request
    );
  }

  /** 服务端流：通知事件 */
  subscribeNotifications(request: EmptyRequest = {}): AsyncIterable<CaptureNotificationEvent> {
    return serverStreamingToAsyncIterable<EmptyRequest, CaptureNotificationEvent>(
      this.client,
      'subscribeNotifications',
      request
    );
  }

  /** 服务端流：流量分片 */
  subscribeTraffic(request: EmptyRequest = {}): AsyncIterable<TrafficChunk> {
    return serverStreamingToAsyncIterable<EmptyRequest, TrafficChunk>(this.client, 'subscribeTraffic', request);
  }
}
