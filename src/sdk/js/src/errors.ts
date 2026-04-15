import * as grpc from '@grpc/grpc-js';

/** SDK 侧可编程错误码（与 gRPC status 对齐命名）。 */
export type SwgErrorCode =
  | 'OK'
  | 'CANCELLED'
  | 'UNKNOWN'
  | 'INVALID_ARGUMENT'
  | 'DEADLINE_EXCEEDED'
  | 'NOT_FOUND'
  | 'ALREADY_EXISTS'
  | 'PERMISSION_DENIED'
  | 'RESOURCE_EXHAUSTED'
  | 'FAILED_PRECONDITION'
  | 'ABORTED'
  | 'OUT_OF_RANGE'
  | 'UNIMPLEMENTED'
  | 'INTERNAL'
  | 'UNAVAILABLE'
  | 'DATA_LOSS'
  | 'UNAUTHENTICATED';

/**
 * 统一封装的 gRPC 调用错误。
 *
 * 与服务端 `GrpcRouteRunner` 映射一致时，
 * 常见情况：`INVALID_ARGUMENT`、`UNAVAILABLE`、`INTERNAL`、`UNAUTHENTICATED`。
 */
export class SwgError extends Error {
  readonly code: SwgErrorCode;
  readonly details?: string;
  override readonly cause?: unknown;

  constructor(message: string, code: SwgErrorCode, options?: { details?: string; cause?: unknown }) {
    super(message, options?.cause !== undefined ? { cause: options.cause } : undefined);
    this.name = 'SwgError';
    this.code = code;
    this.details = options?.details;
    this.cause = options?.cause;
    Object.setPrototypeOf(this, SwgError.prototype);
  }
}

const STATUS_TO_CODE: Record<number, SwgErrorCode> = {
  [grpc.status.OK]: 'OK',
  [grpc.status.CANCELLED]: 'CANCELLED',
  [grpc.status.UNKNOWN]: 'UNKNOWN',
  [grpc.status.INVALID_ARGUMENT]: 'INVALID_ARGUMENT',
  [grpc.status.DEADLINE_EXCEEDED]: 'DEADLINE_EXCEEDED',
  [grpc.status.NOT_FOUND]: 'NOT_FOUND',
  [grpc.status.ALREADY_EXISTS]: 'ALREADY_EXISTS',
  [grpc.status.PERMISSION_DENIED]: 'PERMISSION_DENIED',
  [grpc.status.RESOURCE_EXHAUSTED]: 'RESOURCE_EXHAUSTED',
  [grpc.status.FAILED_PRECONDITION]: 'FAILED_PRECONDITION',
  [grpc.status.ABORTED]: 'ABORTED',
  [grpc.status.OUT_OF_RANGE]: 'OUT_OF_RANGE',
  [grpc.status.UNIMPLEMENTED]: 'UNIMPLEMENTED',
  [grpc.status.INTERNAL]: 'INTERNAL',
  [grpc.status.UNAVAILABLE]: 'UNAVAILABLE',
  [grpc.status.DATA_LOSS]: 'DATA_LOSS',
  [grpc.status.UNAUTHENTICATED]: 'UNAUTHENTICATED',
};

function grpcCodeToSwg(code: number): SwgErrorCode {
  return STATUS_TO_CODE[code] ?? 'UNKNOWN';
}

/** 将 gRPC 错误转为 `SwgError`；若非 `ServiceError` 则包装为 `UNKNOWN`。 */
export function wrapGrpcError(err: unknown): SwgError {
  if (isServiceError(err)) {
    const code = grpcCodeToSwg(err.code);
    return new SwgError(err.message || code, code, {
      details: err.details,
      cause: err,
    });
  }
  if (err instanceof Error) {
    return new SwgError(err.message, 'UNKNOWN', { cause: err });
  }
  return new SwgError(String(err), 'UNKNOWN', { cause: err });
}

function isServiceError(err: unknown): err is grpc.ServiceError {
  return (
    typeof err === 'object' &&
    err !== null &&
    'code' in err &&
    typeof (err as grpc.ServiceError).code === 'number' &&
    'message' in err
  );
}
