# @swg/grpc-client

面向 [SwgServer](https://github.com/ScriptWinGUI/ScriptWinGUI) 的 **Node.js gRPC 客户端 SDK**（`@grpc/grpc-js` + `@grpc/proto-loader`），覆盖 Win32、CV、Input、OCR、Fs、Capture、FlaUI（Automation）共 7 个服务。

## 要求

- **Node.js** ≥ 18
- 目标机器上运行 **SwgServer**（默认监听 `localhost:50051`，**不安全连接**）
- 业务能力（FlaUI、截屏等）依赖 **Windows** 侧；SDK 本身可在任意平台作为客户端使用

## 安装

```bash
npm install @swg/grpc-client
```

## 快速开始

```typescript
import { createSwgClient } from '@swg/grpc-client';

const client = createSwgClient({
  host: 'localhost',
  port: 50051,
  auth: {
    token: '由 SwgServer --generate-token 生成的 plainToken',
  },
});

// 示例：Win32
await client.win32.getForegroundWindow({});

await client.close();
```

## 认证

服务端通过 metadata 读取 **`authorization: Bearer <plainToken>`**。SDK 在传入 `auth.token` 时自动注入；与 C# 端 `AuthInterceptor` 行为一致。

## 配置（SwgClientConfig）

| 字段 | 说明 | 默认 |
|------|------|------|
| `host` | 主机名 | `localhost` |
| `port` | 端口 | `50051` |
| `auth.token` | Bearer plainToken | 无（若服务端要求认证则必填） |
| `protoRoot` | Proto 根目录（含 `win32.proto` 与 `google/protobuf`） | 包内 `proto/` |
| `grpcOptions` | 透传 `@grpc/grpc-js` `ClientOptions` | 无 |

## 服务命名空间

| 属性 | gRPC 服务 | Proto 包 |
|------|-----------|----------|
| `client.win32` | Win32Service | `swg.win32` |
| `client.cv` | CvService | `swg.cv` |
| `client.input` | InputService | `swg.input` |
| `client.ocr` | OcrService | `swg.ocr` |
| `client.fs` | FsService | `swg.fs` |
| `client.capture` | CaptureService | `swg.capture` |
| `client.flaui` | AutomationService（FlaUI） | `swg.flaui` |

请求/响应消息形状以仓库内 **`proto/*.proto`** 为准，请与 `Swg.Grpc/Protos` 保持同步。

## 错误处理

失败时抛出 **`SwgError`**（`import { SwgError } from '@swg/grpc-client'`），`code` 与 gRPC 状态码命名一致（如 `INVALID_ARGUMENT`、`UNAUTHENTICATED`、`UNAVAILABLE`）。服务端业务异常映射见仓库 `GrpcRouteRunner`（`ArgumentException` → `INVALID_ARGUMENT`，`InvalidOperationException` → `UNAVAILABLE` 等）。

## Capture 服务端流

`client.capture.subscribeNotifications()` 与 `subscribeTraffic()` 返回 **`AsyncIterable`**，可用 `for await` 消费：

```typescript
for await (const ev of client.capture.subscribeNotifications({})) {
  console.log(ev);
}
```

## API 文档（本地）

```bash
npm install
npm run docs
```

在浏览器中打开 **`docs/api/index.html`**（`docs/api` 已 `.gitignore`，需本地生成）。

## Proto 同步

SDK 发布包内含 **`proto/`** 副本。若服务端 Proto 变更，请同步更新本包内文件并发新版本，避免字段/方法不一致。

## 示例

见仓库 `examples/`（构建后可用 `node` 运行指向 `dist` 的示例）。

## 许可证

MIT
