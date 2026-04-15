import { describe, expect, it } from 'vitest';
import { promisifyUnary, serverStreamingToAsyncIterable } from '../src/grpc-call.js';

describe('promisifyUnary', () => {
  it('resolves on success', async () => {
    const client = {
      hello(_req: object, cb: (err: unknown, res: unknown) => void) {
        cb(null, { v: 1 });
      },
    };
    const r = await promisifyUnary(client as never, 'hello', {});
    expect(r).toEqual({ v: 1 });
  });

  it('rejects on callback error', async () => {
    const client = {
      hello(_req: object, cb: (err: unknown, res: unknown) => void) {
        cb({ code: 3, message: 'bad' }, null);
      },
    };
    await expect(promisifyUnary(client as never, 'hello', {})).rejects.toMatchObject({
      code: 'INVALID_ARGUMENT',
    });
  });
});

describe('serverStreamingToAsyncIterable', () => {
  it('yields chunks', async () => {
    async function* fakeStream() {
      yield { a: 1 };
      yield { a: 2 };
    }
    const client = {
      sub() {
        return fakeStream();
      },
    };
    const out: unknown[] = [];
    for await (const x of serverStreamingToAsyncIterable(client as never, 'sub', {})) {
      out.push(x);
    }
    expect(out).toEqual([{ a: 1 }, { a: 2 }]);
  });
});
