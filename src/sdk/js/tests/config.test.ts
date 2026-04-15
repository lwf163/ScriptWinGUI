import { describe, expect, it } from 'vitest';
import {
  assertValidConfig,
  resolveConfig,
  resolveDefaultProtoRoot,
  type SwgClientConfig,
} from '../src/config.js';

describe('resolveConfig', () => {
  it('fills defaults', () => {
    const c = resolveConfig({});
    expect(c.host).toBe('localhost');
    expect(c.port).toBe(50051);
    expect(c.protoRoot).toBe(resolveDefaultProtoRoot());
  });

  it('respects overrides', () => {
    const cfg: SwgClientConfig = { host: '127.0.0.1', port: 9999, protoRoot: '/tmp/p' };
    const c = resolveConfig(cfg);
    expect(c.host).toBe('127.0.0.1');
    expect(c.port).toBe(9999);
    expect(c.protoRoot).toBe('/tmp/p');
  });
});

describe('assertValidConfig', () => {
  it('throws on invalid port', () => {
    expect(() => assertValidConfig(resolveConfig({ port: 0 }))).toThrow(/无效 port/);
  });

  it('throws on empty token', () => {
    expect(() => assertValidConfig(resolveConfig({ auth: { token: '   ' } }))).toThrow(/auth.token/);
  });
});
