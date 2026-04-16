# 计划：根据 C# gRPC 注释重写 JS SDK API 文档

## 背景

`Swg.Grpc/Api/` 中的 7 个 C# 门面类已完成详细 XML 文档注释，包含：
- 类级概述（功能范围、Proto 服务对应、异常映射约定）
- 方法级描述（功能说明、参数字段详情、返回值字段、异常场景）

而 `src/sdk/js/src/services/` 中对应的 7 个 TS 客户端类目前只有一行简短 JSDoc（如 `/** 创建监听窗口并返回窗口 ID 与落盘信息。*/`），缺少参数/返回值/异常等详细信息。

## 目标

将 C# 注释中的关键信息翻译为 JSDoc 格式，补充到 JS SDK 的 7 个服务文件中，使 SDK 用户无需查阅 C# 源码即可了解完整 API 语义。

## 实施步骤

### 1. 重写 `capture.ts` — CaptureServiceClient

将 C# `SwgGrpcCaptureApi` 的注释信息映射到以下方法：
- **类级 JSDoc**：补充功能概述（网络流量监听、HTTP/HTTPS 代理、SQLite 持久化、通知捕获、ETW 探针）
- `createListenWindow`：补充请求参数字段说明（StorageDirectory、ProxyListenPort、MITM 证书、流量过滤、通知、ETW 等），返回值字段说明（ListenWindowId、SqlitePath、ProxyListenPort）
- `stopListenWindow`：补充参数（ListenWindowId）、返回值（Stopped、SqlitePath）
- `queryHistory`：补充参数（ListenWindowId、Limit、Offset、BeforeCapturedAtUtc），返回值详细字段（HTTP 交换记录各项）
- `subscribeNotifications`：补充通知事件说明
- `subscribeTraffic`：补充流量分片说明

### 2. 重写 `cv.ts` — CvServiceClient

将 C# `SwgGrpcCvApi` 的注释映射到以下方法：
- **类级 JSDoc**：补充功能概述（模板匹配、像素查找、截图、ROI 一致性检测）
- `findSingleTemplate`：补充 Roi、Template（路径或 Base64）、Threshold 参数说明，返回值（Found、Score、坐标）
- `findOneOfTemplates`：补充 Templates 列表、Preference 策略（FirstQualified/BestScore）
- `findEachTemplateAtLeastOnce`：补充说明每个模板至少匹配一次
- `findPixelsRgb` / `findPixelsRgbMultiple` / `findPixelsHsv`：补充颜色查找说明
- `countPixelsRgb`：像素计数
- `getPixelRgb`：单点 RGB 获取
- `checkWindowRoiConsistency`：DPI 一致性检测
- `captureFullScreen` / `captureRegion`：截图选项说明（OutputKind、ImageFormat、Base64Variant、JpegQuality）

### 3. 重写 `flaui.ts` — FlaUiServiceClient

将 C# `SwgGrpcFlaUiApi` 的注释映射到以下方法：
- **类级 JSDoc**：补充功能概述（会话管理、元素查找/遍历、鼠标点击、焦点控制）
- `createSession`：参数（AutomationType、ExecutablePath、Arguments、LaunchIfNotRunning、ProcessIndex），返回值（SessionId、ProcessId、AutomationType）
- `deleteSession` / `closeApplication` / `killApplication`：会话与应用生命周期
- `waitWhileBusy` / `waitMainHandle`：等待操作
- `getMainWindow` / `getTopLevelWindows`：窗口元素引用
- `getElementInfo`：元素详细信息字段（Name、AutomationId、ClassName、ControlType、Bounds 等）
- `findElement` / `findAllElements`：查找条件字段说明
- `findElementByXPath` / `findAllElementsByXPath`：XPath 查找
- `getChildren` / `focus` / `focusNative` / `setElementForeground`：元素操作
- `click` / `doubleClick` / `rightClick` / `rightDoubleClick`：点击操作

### 4. 重写 `fs.ts` — FsServiceClient

将 C# `SwgGrpcFsApi` 的注释映射到以下方法：
- **类级 JSDoc**：补充功能概述（文件读写、复制/移动/删除、目录操作、文件搜索）
- `readText` / `writeText` / `appendText`：文本操作参数（Path、Text、Encoding）
- `copyFile` / `moveFile` / `deleteFile`：文件操作
- `exists` / `getItemInfo`：文件信息查询（Name、FullPath、IsDirectory、SizeBytes、时间戳、Attributes）
- `createDirectory` / `deleteDirectory` / `listDirectory` / `moveDirectory`：目录操作
- `searchFiles` / `findExeShortcuts`：搜索功能

### 5. 重写 `input.ts` — InputServiceClient

将 C# `SwgGrpcInputApi` 的注释映射到以下方法：
- **类级 JSDoc**：补充功能概述（键盘输入模拟、鼠标操作、光标位置查询）
- 键盘方法组：`typeText`、`typeChar`、`typeKeys`、`typeSimultaneously`、`typeKey`、`press`、`release`、`typeSequence`
- 鼠标方法组：`getCursorPosition`、`setCursorPosition`、`moveTo`、`moveBy`、`getMoveSettings`、`setMoveSettings`、`click`、`down`、`up`、`dragTo`、`dragBy`、`scroll`、`horizontalScroll`
- `wait`：阻塞等待

### 6. 重写 `ocr.ts` — OcrServiceClient

将 C# `SwgGrpcOcrApi` 的注释映射到以下方法：
- **类级 JSDoc**：补充功能概述（OCR 文字识别、表格识别、PaddleSharp/Tesseract 引擎、中/英语言）
- `recognizeScreenStrings`：Roi、Engine、Language、PaddleChineseModel 参数，返回值字段
- `recognizeScreenStringsMatch`：MatchText 过滤
- `recognizeScreenTable`：表格识别（仅 PaddleSharp）
- `recognizeImageStrings` / `recognizeImageTable`：图像文件识别
- `recognizeScreenQuickTable` / `recognizeImageQuickTable`：QuickTable 算法

### 7. 重写 `win32.ts` — Win32ServiceClient

将 C# `SwgGrpcWin32Api` 的注释映射到以下方法：
- **类级 JSDoc**：补充功能概述（窗口管理、进程管理、剪贴板、系统信息、消息发送、控件文本）
- 窗口方法组：`findWindow`、`getForegroundWindow`、`setForegroundWindow`、`getWindowInfo`、`setWindowPositionResize`、`setWindowState`、`closeWindow`、`getWindowProcessId`、`enumChildWindows`、`findChildWindow`
- 进程方法组：`startProcess`、`killProcess`、`getCurrentProcessId`、`processExists`、`processWaitExit`
- 剪贴板方法组：`getClipboardText`、`setClipboardText`、`clearClipboard`
- 系统信息方法组：`getMainScreen`、`getVirtualScreen`、`getSystemDpi`、`getCursorPosition`、`getForegroundWindowInfo`
- 消息方法组：`sendMessage`、`postMessage`、`windowFromPoint`、`sendKeys`、`getControlText`、`sendWmCommand`

### 8. 补充 `client.ts` — SwgClient 类文档

- 完善 `SwgClient` 类级 JSDoc，添加 7 个服务属性的简要说明
- 完善 `createSwgClient` 函数的参数说明

## 文档格式约定

- 使用 JSDoc `@param` 标注请求参数中的关键字段（使用 `request.xxx` 形式标注嵌套字段）
- 使用 `@returns` 标注返回值结构
- 使用 `@throws` 标注可能的 gRPC 错误码（映射自 C# 异常约定）
- 保持中文注释风格，与现有代码一致
- 注释精简但信息完整，避免过度冗长
