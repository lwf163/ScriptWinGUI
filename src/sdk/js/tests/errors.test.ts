import * as grpc from '@grpc/grpc-js';
import { describe, expect, it } from 'vitest';
import { SwgError, wrapGrpcError } from '../src/errors.js';

describe('wrapGrpcError', () => {
  it('maps ServiceError to SwgError', () => {
    const err = Object.assign(new Error('bad'), {
      code: grpc.status.INVALID_ARGUMENT,
      details: 'x',
    }) as grpc.ServiceError;
    const w = wrapGrpcError(err);
    expect(w).toBeInstanceOf(SwgError);
    expect(w.code).toBe('INVALID_ARGUMENT');
    expect(w.message).toContain('bad');
  });

  it('wraps generic Error', () => {
    const w = wrapGrpcError(new Error('oops'));
    expect(w.code).toBe('UNKNOWN');
  });
});
