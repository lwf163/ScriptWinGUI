import * as grpc from '@grpc/grpc-js';
import { wrapGrpcError } from './errors.js';

type UnaryMethod<TReq extends object, TRes> = (
  request: TReq,
  callback: grpc.requestCallback<TRes>
) => grpc.ClientUnaryCall;

/**
 * 将 unary RPC 转为 Promise；方法名为 proto-loader 生成的 camelCase。
 */
export function promisifyUnary<TReq extends object, TRes>(
  client: grpc.Client,
  methodName: string,
  request: TReq
): Promise<TRes> {
  return new Promise((resolve, reject) => {
    const fn = (client as unknown as Record<string, unknown>)[methodName];
    if (typeof fn !== 'function') {
      reject(new Error(`未知 RPC 方法: ${methodName}`));
      return;
    }
    (fn as UnaryMethod<TReq, TRes>).call(client, request, (err, response) => {
      if (err) reject(wrapGrpcError(err));
      else if (response === undefined) reject(new Error(`RPC 响应为空: ${methodName}`));
      else resolve(response);
    });
  });
}

type ServerStreamingMethod<TReq extends object, TItem> = (request: TReq) => grpc.ClientReadableStream<TItem>;

/**
 * 服务端流式 RPC → AsyncIterable（方法名为 camelCase）。
 */
export async function* serverStreamingToAsyncIterable<TReq extends object, TItem>(
  client: grpc.Client,
  methodName: string,
  request: TReq
): AsyncIterable<TItem> {
  const fn = (client as unknown as Record<string, unknown>)[methodName];
  if (typeof fn !== 'function') {
    throw new Error(`未知 RPC 方法: ${methodName}`);
  }
  const stream = (fn as ServerStreamingMethod<TReq, TItem>).call(client, request);
  try {
    for await (const chunk of stream) {
      yield chunk;
    }
  } catch (e) {
    throw wrapGrpcError(e);
  }
}
