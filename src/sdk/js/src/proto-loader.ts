import { join } from 'node:path';
import * as protoLoader from '@grpc/proto-loader';

const PROTO_FILES = [
  'win32.proto',
  'cv.proto',
  'input.proto',
  'ocr.proto',
  'fs.proto',
  'capture.proto',
  'flaui.proto',
] as const;

/**
 * 加载 Swg 全部服务定义（含 `google/protobuf` 依赖，includeDirs 为 proto 根目录）。
 */
export function loadSwgPackageDefinition(protoRoot: string) {
  return protoLoader.loadSync(
    PROTO_FILES.map((f) => join(protoRoot, f)),
    {
      keepCase: false,
      longs: String,
      enums: String,
      defaults: true,
      oneofs: true,
      includeDirs: [protoRoot],
    }
  );
}
