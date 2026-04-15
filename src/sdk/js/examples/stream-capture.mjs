/**
 * 构建后运行: node examples/stream-capture.mjs
 * Capture 流式 RPC 示例（需有效 token 与服务端支持）。
 */
import { createSwgClient } from '../dist/index.mjs';

const token = process.env.SWG_TOKEN;
const client = createSwgClient(
  token ? { auth: { token } } : {}
);

try {
  for await (const chunk of client.capture.subscribeTraffic({})) {
    console.log('traffic', chunk);
    break;
  }
} catch (e) {
  console.error(e);
} finally {
  await client.close();
}
