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
 * Capture gRPC 客户端（Proto 服务 `swg.capture.CaptureService`）。
 *
 * 提供网络流量监听窗口的创建/停止、历史记录查询、通知事件订阅和流量分片订阅。
 * 监听窗口会启动本地代理服务器拦截 HTTP/HTTPS 流量并持久化到 SQLite，
 * 同时可选启用 Windows 通知捕获和 ETW 探针。
 */
export class CaptureServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /**
   * 创建一个监听窗口实例，用于捕获 HTTP 流量和/或 Windows 通知事件。
   *
   * @param request 请求参数：
   *   - `storageDirectory`（string，可选）：SQLite 数据库存储目录，为空则使用默认临时目录
   *   - `proxyListenPort`（number，可选）：代理监听端口，0 或不填则自动分配
   *   - `maxBodyBytesPerPart`（number，可选）：单个请求/响应体的最大字节数
   *   - `flushIntervalMs`（number，可选）：批量写入数据库的刷新间隔（毫秒）
   *   - `flushBatchMaxRows`（number，可选）：单批次最大行数
   *   - `flushBatchMaxBytes`（number，可选）：单批次最大字节数
   *   - `trafficAllowAll`（boolean）：是否允许捕获所有流量（不过滤）
   *   - `trafficHostContains`（string[]）：流量过滤规则 - 主机名包含关键词列表
   *   - `trafficPathPrefixes`（string[]）：流量过滤规则 - 路径前缀列表
   *   - `mitmUserTrustRoot`（boolean）：是否在当前用户存储安装 MITM 根证书
   *   - `mitmMachineTrustRoot`（boolean）：是否在计算机存储安装 MITM 根证书
   *   - `mitmTrustRootAsAdministrator`（boolean）：是否以管理员权限安装根证书
   *   - `enableNotifications`（boolean）：是否启用 Windows 通知捕获
   *   - `notificationDebounceMs`（number，可选）：通知事件去抖间隔（毫秒）
   *   - `notificationProcessNameContains`（string[]）：通知过滤 - 进程名包含关键词
   *   - `notificationTitleContains`（string，可选）：通知过滤 - 标题包含关键词
   *   - `hookWindowEventTypes`（string[]）：窗口事件钩子类型列表
   *   - `enableEtwProbe`（boolean）：是否启用 ETW 探针
   *   - `etwProviderNames`（string[]）：ETW 音频轨提供者名称列表
   *   - `etwWindowProviderNames`（string[]）：ETW 窗口轨提供者名称列表
   *   - `etwWindowEventTypes`（string[]）：ETW 窗口事件类型列表
   *   - `etwMatchAnyKeyword`（number，可选）：ETW 关键字掩码
   *   - `etwLevel`（number，可选）：ETW 事件级别
   *   - `etwQueueCapacity`（number，可选）：ETW 音频轨队列容量
   *   - `etwWindowQueueCapacity`（number，可选）：ETW 窗口轨队列容量
   *   - `trafficStatsIntervalMs`（number，可选）：流量统计上报间隔（毫秒）
   * @returns 包含以下字段：
   *   - `listenWindowId`（string）：监听窗口的唯一标识（GUID 格式），后续操作需使用此 ID
   *   - `sqlitePath`（string）：SQLite 数据库文件完整路径
   *   - `proxyListenPort`（number）：代理实际监听的端口号
   * @throws gRPC `InvalidArgument` — EtwWindowProviderNames 与 EtwWindowEventTypes 未同时提供，或 HookWindowEventTypes 包含无效事件类型
   */
  createListenWindow(request: CaptureCreateListenWindowRequest): Promise<CaptureCreateListenWindowResponse> {
    return promisifyUnary<CaptureCreateListenWindowRequest, CaptureCreateListenWindowResponse>(
      this.client,
      'createListenWindow',
      request
    );
  }

  /**
   * 停止并销毁指定监听窗口，释放代理端口和所有关联资源。
   *
   * @param request 请求参数：
   *   - `listenWindowId`（string，必填）：由 `createListenWindow` 返回的监听窗口 ID
   * @returns 包含以下字段：
   *   - `stopped`（boolean）：是否成功停止（若 ID 不存在则为 false）
   *   - `sqlitePath`（string）：已关闭窗口对应的 SQLite 数据库路径，未停止时为空字符串
   */
  stopListenWindow(request: CaptureStopListenWindowRequest): Promise<CaptureStopListenWindowResponse> {
    return promisifyUnary<CaptureStopListenWindowRequest, CaptureStopListenWindowResponse>(
      this.client,
      'stopListenWindow',
      request
    );
  }

  /**
   * 分页查询指定监听窗口捕获的 HTTP 交换记录，结果按捕获时间倒序排列。
   *
   * @param request 请求参数：
   *   - `listenWindowId`（string，必填）：监听窗口 ID
   *   - `limit`（number）：单页最大返回条数
   *   - `offset`（number）：偏移量
   *   - `beforeCapturedAtUtc`（string，可选）：ISO 8601 时间戳，仅返回此时间之前捕获的记录
   * @returns 包含 `items`（数组），每条记录含：
   *   - `id`（number）：记录唯一 ID
   *   - `capturedAt`（string）：捕获时间（ISO 8601 格式）
   *   - `method`（string）：HTTP 方法（GET/POST 等）
   *   - `scheme`（string）：协议（http/https）
   *   - `host`（string）：目标主机名
   *   - `port`（number）：目标端口
   *   - `path`（string）：请求路径
   *   - `queryText`（string）：查询字符串
   *   - `urlDisplay`（string）：用于展示的完整 URL
   *   - `hasResponseStatus` / `responseStatus`（number）：HTTP 响应状态码
   *   - `hasDurationMs` / `durationMs`（number）：请求耗时毫秒数
   *   - `errorText`（string）：错误信息（无错误时为空）
   *   - `hasClientProcessId` / `clientProcessId`（number）：发起请求的客户端进程 ID
   *   - `clientProcessName`（string）：发起请求的客户端进程名称
   * @throws gRPC `Unavailable` — 指定的 listenWindowId 不存在或已停止
   */
  queryHistory(request: CaptureHistoryQueryRequest): Promise<CaptureHistoryQueryResponse> {
    return promisifyUnary<CaptureHistoryQueryRequest, CaptureHistoryQueryResponse>(
      this.client,
      'queryHistory',
      request
    );
  }

  /**
   * 订阅 Windows 通知事件流（服务端流式 RPC）。
   *
   * @param request 空请求（默认 `{}`）
   * @returns 异步迭代器，每次迭代产生一个 `CaptureNotificationEvent`
   */
  subscribeNotifications(request: EmptyRequest = {}): AsyncIterable<CaptureNotificationEvent> {
    return serverStreamingToAsyncIterable<EmptyRequest, CaptureNotificationEvent>(
      this.client,
      'subscribeNotifications',
      request
    );
  }

  /**
   * 订阅流量数据分片流（服务端流式 RPC）。
   *
   * @param request 空请求（默认 `{}`）
   * @returns 异步迭代器，每次迭代产生一个 `TrafficChunk`
   */
  subscribeTraffic(request: EmptyRequest = {}): AsyncIterable<TrafficChunk> {
    return serverStreamingToAsyncIterable<EmptyRequest, TrafficChunk>(this.client, 'subscribeTraffic', request);
  }
}
