import { existsSync, statSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';
import type * as grpc from '@grpc/grpc-js';

/**
 * SwgServer 客户端配置。
 */
export interface SwgClientConfig {
  /** 主机名，默认 `localhost` */
  host?: string;
  /** 端口，默认 `50051` */
  port?: number;
  /** Bearer Token（plainToken），由 SwgServer `--generate-token` 生成 */
  auth?: {
    token: string;
  };
  /**
   * Proto 根目录（含 `win32.proto` 等及 `google/protobuf`）。
   * 若不指定，默认使用包内 `proto/`（相对发布包 `dist` 的上一级）。
   */
  protoRoot?: string;
  /** 透传给 gRPC Client 的选项（如拦截器会与内置认证拦截器合并） */
  grpcOptions?: grpc.ClientOptions;
}

const DEFAULT_HOST = 'localhost';
const DEFAULT_PORT = 50051;

/**
 * 解析默认 proto 根目录（适用于从 `dist/` 加载的已编译入口）。
 */
export function resolveDefaultProtoRoot(): string {
  const here = dirname(fileURLToPath(import.meta.url));
  return join(here, '..', 'proto');
}

/**
 * 合并用户配置与默认值。
 */
export function resolveConfig(config: SwgClientConfig): Required<
  Pick<SwgClientConfig, 'host' | 'port' | 'protoRoot'>
> &
  SwgClientConfig {
  const protoRoot = config.protoRoot ?? resolveDefaultProtoRoot();
  return {
    ...config,
    host: config.host ?? DEFAULT_HOST,
    port: config.port ?? DEFAULT_PORT,
    protoRoot,
  };
}

export function assertValidConfig(config: ReturnType<typeof resolveConfig>): void {
  if (!protoRootExists(config.protoRoot)) {
    throw new Error(
      `Proto 目录不存在或不可读: ${config.protoRoot}。请设置 protoRoot 或确保 npm 包内包含 proto。`
    );
  }
  if (config.port < 1 || config.port > 65535) {
    throw new Error(`无效 port: ${config.port}`);
  }
  if (config.auth?.token !== undefined && config.auth.token.trim() === '') {
    throw new Error('auth.token 不能为空字符串');
  }
}

function protoRootExists(path: string): boolean {
  try {
    return existsSync(path) && statSync(path).isDirectory();
  } catch {
    return false;
  }
}
