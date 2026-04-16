import * as grpc from '@grpc/grpc-js';
import { createBearerAuthInterceptor } from './auth.js';
import { assertValidConfig, resolveConfig, type SwgClientConfig } from './config.js';
import { loadSwgPackageDefinition } from './proto-loader.js';
import { CaptureServiceClient } from './services/capture.js';
import { CvServiceClient } from './services/cv.js';
import { FlaUiServiceClient } from './services/flaui.js';
import { FsServiceClient } from './services/fs.js';
import { InputServiceClient } from './services/input.js';
import { OcrServiceClient } from './services/ocr.js';
import { Win32ServiceClient } from './services/win32.js';

type ServiceCtor = new (
  address: string,
  credentials: grpc.ChannelCredentials,
  options?: grpc.ClientOptions
) => grpc.Client;

interface SwgGrpcRoot {
  swg: {
    win32: { Win32Service: ServiceCtor };
    cv: { CvService: ServiceCtor };
    input: { InputService: ServiceCtor };
    ocr: { OcrService: ServiceCtor };
    fs: { FsService: ServiceCtor };
    capture: { CaptureService: ServiceCtor };
    flaui: { AutomationService: ServiceCtor };
  };
}

function mergeClientOptions(token: string | undefined, user?: grpc.ClientOptions): grpc.ClientOptions {
  const auth = token ? [createBearerAuthInterceptor(token)] : [];
  const rest = user?.interceptors ?? [];
  const mergedInterceptors = [...auth, ...rest];
  return {
    ...user,
    ...(mergedInterceptors.length > 0 ? { interceptors: mergedInterceptors } : {}),
  };
}

export type ResolvedSwgClientConfig = ReturnType<typeof resolveConfig>;

/**
 * 聚合 7 个 gRPC 服务的客户端；默认使用不安全连接（与 SwgServer 一致）。
 *
 * 服务属性说明：
 * - `win32`：Win32 系统操作（窗口管理、进程管理、剪贴板、系统信息、消息发送）
 * - `cv`：计算机视觉（模板匹配、像素查找/计数、屏幕截图、ROI 一致性检测）
 * - `input`：输入模拟（键盘输入、鼠标操作、光标位置查询）
 * - `ocr`：光学字符识别（文字识别、表格识别、QuickTable 快速表格）
 * - `fs`：文件系统操作（文件读写、复制/移动/删除、目录操作、文件搜索）
 * - `capture`：流量捕获（监听窗口管理、HTTP 交换记录查询、通知/流量流订阅）
 * - `flaui`：FlaUI UI 自动化（会话管理、元素查找/遍历、鼠标点击、焦点控制）
 */
export class SwgClient {
  readonly win32: Win32ServiceClient;
  readonly cv: CvServiceClient;
  readonly input: InputServiceClient;
  readonly ocr: OcrServiceClient;
  readonly fs: FsServiceClient;
  readonly capture: CaptureServiceClient;
  readonly flaui: FlaUiServiceClient;

  private readonly channels: grpc.Client[] = [];

  constructor(config: ResolvedSwgClientConfig) {
    const address = `${config.host}:${config.port}`;
    const creds = grpc.credentials.createInsecure();
    const opts = mergeClientOptions(config.auth?.token, config.grpcOptions);

    const root = grpc.loadPackageDefinition(
      loadSwgPackageDefinition(config.protoRoot)
    ) as unknown as SwgGrpcRoot;

    const mk = (Ctor: ServiceCtor) => {
      const c = new Ctor(address, creds, opts);
      this.channels.push(c);
      return c;
    };

    this.win32 = new Win32ServiceClient(mk(root.swg.win32.Win32Service));
    this.cv = new CvServiceClient(mk(root.swg.cv.CvService));
    this.input = new InputServiceClient(mk(root.swg.input.InputService));
    this.ocr = new OcrServiceClient(mk(root.swg.ocr.OcrService));
    this.fs = new FsServiceClient(mk(root.swg.fs.FsService));
    this.capture = new CaptureServiceClient(mk(root.swg.capture.CaptureService));
    this.flaui = new FlaUiServiceClient(mk(root.swg.flaui.AutomationService));
  }

  /** 关闭底层 gRPC Channel（各服务独立 Client 会各自关闭）。 */
  async close(): Promise<void> {
    for (const c of this.channels) {
      c.close();
    }
  }
}

/**
 * 创建 {@link SwgClient}。
 */
export function createSwgClient(config: SwgClientConfig = {}): SwgClient {
  const resolved = resolveConfig(config);
  assertValidConfig(resolved);
  return new SwgClient(resolved);
}
