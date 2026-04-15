import * as grpc from '@grpc/grpc-js';

/**
 * 注入 `authorization: Bearer <token>`，与服务端 `AuthInterceptor` 行为一致。
 */
export function createBearerAuthInterceptor(token: string): grpc.Interceptor {
  return (options, nextCall) =>
    new grpc.InterceptingCall(nextCall(options), {
      start(metadata, listener, next) {
        metadata.set('authorization', `Bearer ${token}`);
        next(metadata, listener);
      },
    });
}
