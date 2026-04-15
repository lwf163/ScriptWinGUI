import { describe, expect, it } from 'vitest';
import { createSwgClient } from '../src/client.js';
import { resolveDefaultProtoRoot } from '../src/config.js';

describe('createSwgClient', () => {
  it('constructs all service namespaces', async () => {
    const client = createSwgClient({ protoRoot: resolveDefaultProtoRoot() });
    expect(client.win32).toBeDefined();
    expect(client.cv).toBeDefined();
    expect(client.input).toBeDefined();
    expect(client.ocr).toBeDefined();
    expect(client.fs).toBeDefined();
    expect(client.capture).toBeDefined();
    expect(client.flaui).toBeDefined();
    await client.close();
  });
});
