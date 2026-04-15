/**
 * 构建后运行: node examples/basic-usage.mjs
 * 需本机 SwgServer 与有效 token（按需修改）。
 */
import { createSwgClient } from '../dist/index.mjs';

const client = createSwgClient({
  host: 'localhost',
  port: 50051,
  // auth: { token: 'your-plain-token' },
});

try {
  const res = await client.win32.getVirtualScreen({});
  console.log('getVirtualScreen', res);
} catch (e) {
  console.error(e);
} finally {
  await client.close();
}
