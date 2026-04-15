/**
 * @packageDocumentation
 * SwgServer 的 Node.js gRPC 客户端 SDK（`@grpc/grpc-js` + `@grpc/proto-loader`）。
 *
 * Proto 定义见包内 `proto/`，与服务端 `Swg.Grpc/Protos` 应保持同步。
 */

export { createSwgClient, SwgClient, type ResolvedSwgClientConfig } from './client.js';
export type { SwgClientConfig } from './config.js';
export type * from './proto-types.js';
export { resolveDefaultProtoRoot, resolveConfig, assertValidConfig } from './config.js';
export { createBearerAuthInterceptor } from './auth.js';
export { SwgError, wrapGrpcError, type SwgErrorCode } from './errors.js';

export { Win32ServiceClient } from './services/win32.js';
export { CvServiceClient } from './services/cv.js';
export { InputServiceClient } from './services/input.js';
export { OcrServiceClient } from './services/ocr.js';
export { FsServiceClient } from './services/fs.js';
export { FlaUiServiceClient } from './services/flaui.js';
export { CaptureServiceClient } from './services/capture.js';
