/**
 * 构建后运行: node examples/with-auth.mjs
 * 设置环境变量 SWG_TOKEN 为 plainToken。
 */
import { createSwgClient } from '../dist/index.mjs';

const token = process.env.SWG_TOKEN;
if (!token) {
  console.error('请设置环境变量 SWG_TOKEN');
  process.exit(1);
}

const client = createSwgClient({
  auth: { token },
});

try {
  const w = await client.win32.getForegroundWindow({});
  console.log('foreground', w);
} catch (e) {
  console.error(e);
} finally {
  await client.close();
}
