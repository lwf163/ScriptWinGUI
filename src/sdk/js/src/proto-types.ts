/**
 * SDK 暴露的 Proto 类型定义（Protocol Buffers，协议缓冲）映射。
 *
 * 说明：
 * - 字段命名遵循 proto-loader 的默认 camelCase 结果。
 * - 所有字段按 proto3 语义建模；未显式 oneof 的值字段通常可省略（用 `?` 表示）。
 * - 字段注释遵循“用途 + 单位/范围 + 可选标记语义（如 `hasXxx`）”格式，便于 TypeDoc 展示。
 * - 首次出现的关键术语保留英文原文并附中文释义。
 */

/** 对应 `google.protobuf.Empty`。 */
export type EmptyRequest = Record<string, never>;

/** 对应 `google.protobuf.Timestamp`。 */
export interface ProtoTimestamp {
  /** Unix 时间戳秒部分。 */
  seconds?: string | number;
  /** 纳秒部分，范围 `0-999,999,999`。 */
  nanos?: number;
}

/** 通用成功响应。 */
export interface OkResponse {
  /** 是否执行成功。 */
  ok?: boolean;
}

/** 通用矩形区域。 */
export interface Roi {
  /** 左上角 X 坐标。 */
  left?: number;
  /** 左上角 Y 坐标。 */
  top?: number;
  /** 宽度（像素）。 */
  width?: number;
  /** 高度（像素）。 */
  height?: number;
}

/** 通用点坐标。 */
export interface ScreenPoint {
  /** X 坐标（像素）。 */
  x?: number;
  /** Y 坐标（像素）。 */
  y?: number;
}

/** RGB 颜色。 */
export interface Rgb {
  /** 红色通道值，范围 `0-255`。 */
  r?: number;
  /** 绿色通道值，范围 `0-255`。 */
  g?: number;
  /** 蓝色通道值，范围 `0-255`。 */
  b?: number;
}

/** HSV 颜色。 */
export interface Hsv {
  /** Hue（色相）分量。 */
  h?: number;
  /** Saturation（饱和度）分量。 */
  s?: number;
  /** Value（明度）分量。 */
  v?: number;
}

/** 屏幕截图输出选项。 */
export interface ScreenshotOptions {
  /** 输出类型，例如文件路径或 Base64 文本。 */
  outputKind?: string;
  /** 当输出到文件时的目标路径。 */
  targetFilePath?: string;
  /** 图像格式，如 `png`/`jpeg`。 */
  imageFormat?: string;
  /** Base64 编码变体。 */
  base64Variant?: string;
  /** JPEG 质量，常见范围 `1-100`。 */
  jpegQuality?: number;
}

/** Win32 窗口矩形。 */
export interface WindowRectDto {
  /** 左边界 X。 */
  left?: number;
  /** 上边界 Y。 */
  top?: number;
  /** 右边界 X。 */
  right?: number;
  /** 下边界 Y。 */
  bottom?: number;
  /** 窗口宽度。 */
  width?: number;
  /** 窗口高度。 */
  height?: number;
}

export interface WindowHandleResponse {
  /** 窗口句柄字符串。 */
  windowHandle?: string;
}

export interface WindowInfoResponse {
  /** 窗口句柄字符串。 */
  windowHandle?: string;
  /** 窗口标题文本。 */
  title?: string;
  /** 窗口类名（Window Class Name）。 */
  className?: string;
  /** 所属进程 ID。 */
  processId?: number;
  /** 窗口矩形信息。 */
  rect?: WindowRectDto;
}

export interface FindWindowRequest {
  /** 标题包含匹配。 */
  titleContains?: string;
  /** 类名精确匹配。 */
  classNameEquals?: string;
  /** 目标进程 ID。 */
  processId?: number;
  /** 是否启用 `processId` 条件。 */
  hasProcessId?: boolean;
  /** 是否仅匹配可见窗口。 */
  visibleOnly?: boolean;
}

export interface FindChildWindowRequest {
  /** 父窗口句柄。 */
  parentWindowHandle?: string;
  /** 子窗口标题包含匹配。 */
  titleContains?: string;
  /** 子窗口类名精确匹配。 */
  classNameEquals?: string;
  /** 是否仅匹配可见子窗口。 */
  visibleOnly?: boolean;
}

export interface WindowInfoRequest {
  /** 目标窗口句柄。 */
  windowHandle?: string;
}

export interface EnumChildWindowsRequest {
  /** 需要枚举的父窗口句柄。 */
  parentWindowHandle?: string;
}

export interface ChildWindowHandlesResponse {
  /** 子窗口句柄集合。 */
  windowHandles?: string[];
}

export interface ForegroundWindowRequest {
  /** 目标前台窗口句柄。 */
  windowHandle?: string;
}

export interface WindowPositionResizeRequest {
  /** 目标窗口句柄。 */
  windowHandle?: string;
  /** 新位置左上角 X。 */
  left?: number;
  /** 新位置左上角 Y。 */
  top?: number;
  /** 新宽度（像素）。 */
  width?: number;
  /** 新高度（像素）。 */
  height?: number;
}

export interface WindowStateRequest {
  /** 目标窗口句柄。 */
  windowHandle?: string;
  /** 目标状态，如 `minimized`/`maximized`。 */
  state?: string;
}

export interface CloseWindowRequest {
  /** 需要关闭的窗口句柄。 */
  windowHandle?: string;
}

export interface WindowProcessIdRequest {
  /** 目标窗口句柄。 */
  windowHandle?: string;
}

export interface WindowProcessIdResponse {
  /** 进程 ID。 */
  processId?: number;
}

export interface ProcessStartRequest {
  /** 可执行文件路径。 */
  executablePath?: string;
  /** 启动参数字符串。 */
  arguments?: string;
}

export interface ProcessStartResponse {
  /** 新进程 ID。 */
  processId?: number;
}

export interface ProcessKillRequest {
  /** 要终止的进程 ID。 */
  processId?: number;
}

export interface ProcessCurrentIdResponse {
  /** 当前进程 ID。 */
  processId?: number;
}

export interface ProcessExistsRequest {
  /** 待检查进程 ID。 */
  processId?: number;
}

export interface ProcessExistsResponse {
  /** 进程是否存在。 */
  exists?: boolean;
}

export interface ProcessWaitExitRequest {
  /** 待等待进程 ID。 */
  processId?: number;
  /** 超时毫秒数（ms）。 */
  timeoutMs?: number;
  /** 是否启用 `timeoutMs`。 */
  hasTimeoutMs?: boolean;
}

export interface ProcessWaitExitResponse {
  /** 进程是否已退出。 */
  exited?: boolean;
}

export interface ClipboardTextResponse {
  /** 剪贴板文本。 */
  text?: string;
}

export interface ClipboardTextSetRequest {
  /** 要写入的剪贴板文本。 */
  text?: string;
}

export interface ClipboardClearResponse {
  /** 清空是否成功。 */
  ok?: boolean;
}

export interface MainScreenResponse {
  /** 主屏幕宽度（像素）。 */
  width?: number;
  /** 主屏幕高度（像素）。 */
  height?: number;
}

export interface VirtualScreenResponse {
  /** 虚拟屏幕左上角 X。 */
  x?: number;
  /** 虚拟屏幕左上角 Y。 */
  y?: number;
  /** 虚拟屏幕宽度。 */
  width?: number;
  /** 虚拟屏幕高度。 */
  height?: number;
}

export interface SystemDpiResponse {
  /** X 轴 DPI（每英寸点数）。 */
  dpiX?: number;
  /** Y 轴 DPI（每英寸点数）。 */
  dpiY?: number;
}

export interface CursorPositionResponse {
  /** 光标 X 坐标。 */
  x?: number;
  /** 光标 Y 坐标。 */
  y?: number;
}

export interface ForegroundWindowInfoResponse {
  /** 前台窗口详细信息。 */
  window?: WindowInfoResponse;
}

export interface WindowFromPointRequest {
  /** 屏幕坐标 X。 */
  x?: number;
  /** 屏幕坐标 Y。 */
  y?: number;
}

export interface WindowHandleAtPointResponse {
  /** 对应坐标下的窗口句柄。 */
  windowHandle?: string;
}

export interface WindowMessageRequest {
  /** 目标窗口句柄。 */
  windowHandle?: string;
  /** Windows 消息编号（Message ID）。 */
  msg?: number;
  /** 消息 `wParam` 参数。 */
  wParam?: string;
  /** 消息 `lParam` 参数。 */
  lParam?: string;
}

export interface WindowMessageSendResponse {
  /** SendMessage 返回值。 */
  result?: string | number;
}

export interface WindowMessagePostResponse {
  /** PostMessage 是否投递成功。 */
  ok?: boolean;
}

export interface WindowKeysSendRequest {
  /** 目标窗口句柄。 */
  windowHandle?: string;
  /** 键序列，例如 `['CTRL', 'S']`。 */
  keys?: string[];
}

export interface WindowKeysSendResponse {
  /** 按键发送是否成功。 */
  ok?: boolean;
}

export interface ControlTextGetRequest {
  /** 控件句柄。 */
  controlHandle?: string;
  /** 最大读取长度。 */
  maxLength?: number;
  /** 是否启用 `maxLength`。 */
  hasMaxLength?: boolean;
}

export interface ControlTextGetResponse {
  /** 控件文本。 */
  text?: string;
}

export interface WmCommandSendRequest {
  /** 目标窗口句柄。 */
  targetWindowHandle?: string;
  /** 命令 ID（commandId）。 */
  commandId?: number;
  /** 通知码（notificationCode）。 */
  notificationCode?: number;
  /** 发送方窗口句柄。 */
  senderHandle?: string;
}

export interface WmCommandSendResponse {
  /** WM_COMMAND 返回值。 */
  result?: string | number;
}

export interface FindSingleTemplateRequest {
  /** 检测区域（Region of Interest，感兴趣区域）。 */
  roi?: Roi;
  /** 模板图像（通常是 Base64 或路径）。 */
  template?: string;
  /** 匹配阈值，越高越严格。 */
  threshold?: number;
}

export interface FindOneOfTemplatesRequest {
  /** 检测区域。 */
  roi?: Roi;
  /** 候选模板集合。 */
  templates?: string[];
  /** 匹配阈值。 */
  threshold?: number;
  /** 命中偏好策略。 */
  preference?: string;
}

export interface FindAllTemplatesRequest {
  /** 检测区域。 */
  roi?: Roi;
  /** 候选模板集合。 */
  templates?: string[];
  /** 匹配阈值。 */
  threshold?: number;
}

export interface FindPixelsRgbRequest {
  /** 检测区域。 */
  roi?: Roi;
  /** 目标 RGB 颜色。 */
  rgb?: Rgb;
}

export interface FindPixelsRgbMultipleRequest {
  /** 检测区域。 */
  roi?: Roi;
  /** 多个目标 RGB 颜色。 */
  rgbColors?: Rgb[];
}

export interface FindPixelsHsvRequest {
  /** 检测区域。 */
  roi?: Roi;
  /** 目标 HSV 颜色。 */
  hsv?: Hsv;
}

export interface CountPixelsRgbRequest {
  /** 检测区域。 */
  roi?: Roi;
  /** 目标 RGB 颜色。 */
  rgb?: Rgb;
}

export interface GetPixelRgbRequest {
  /** 像素点 X 坐标。 */
  x?: number;
  /** 像素点 Y 坐标。 */
  y?: number;
}

export interface WindowRoiConsistencyRequest {
  /** 目标窗口句柄。 */
  windowHandle?: string;
}

export interface CaptureFullScreenRequest {
  /** 截图输出选项。 */
  options?: ScreenshotOptions;
}

export interface CaptureRegionRequest {
  /** 截图区域。 */
  roi?: Roi;
  /** 截图输出选项。 */
  options?: ScreenshotOptions;
}

export interface FindSingleTemplateResponse {
  /** 是否找到模板。 */
  found?: boolean;
  /** 匹配得分。 */
  score?: number;
  /** 命中区域左上角 X。 */
  left?: number;
  /** 命中区域左上角 Y。 */
  top?: number;
  /** 命中区域宽度。 */
  width?: number;
  /** 命中区域高度。 */
  height?: number;
}

export interface FindOneOfTemplatesResponse {
  /** 是否找到任一模板。 */
  found?: boolean;
  /** 命中的模板索引。 */
  templateIndex?: number;
  /** 匹配得分。 */
  score?: number;
  /** 命中区域左上角 X。 */
  left?: number;
  /** 命中区域左上角 Y。 */
  top?: number;
  /** 命中区域宽度。 */
  width?: number;
  /** 命中区域高度。 */
  height?: number;
}

export interface TemplateMatchItem {
  /** 模板索引。 */
  templateIndex?: number;
  /** 当前模板是否命中。 */
  found?: boolean;
  /** 当前模板匹配分数。 */
  score?: number;
  /** 命中区域左上角 X。 */
  left?: number;
  /** 命中区域左上角 Y。 */
  top?: number;
  /** 命中区域宽度。 */
  width?: number;
  /** 命中区域高度。 */
  height?: number;
}

export interface FindAllTemplatesResponse {
  /** 所有模板是否都命中。 */
  allFound?: boolean;
  /** 每个模板的命中明细。 */
  items?: TemplateMatchItem[];
}

export interface ScreenPointsResponse {
  /** 像素点集合。 */
  points?: ScreenPoint[];
}

export interface CountPixelsRgbResponse {
  /** 命中像素数量。 */
  count?: number;
}

export interface PixelRgbResponse {
  /** 是否成功获取像素。 */
  success?: boolean;
  /** 红色分量。 */
  r?: number;
  /** 绿色分量。 */
  g?: number;
  /** 蓝色分量。 */
  b?: number;
}

export interface WindowRoiConsistencyResponse {
  /** 一致性检查是否执行成功。 */
  success?: boolean;
  /** 服务端使用的 ROI。 */
  roi?: Roi;
  /** 捕获宽度。 */
  capturedWidth?: number;
  /** 捕获高度。 */
  capturedHeight?: number;
  /** 窗口 DPI。 */
  windowDpi?: number;
  /** 像素尺寸是否匹配。 */
  isPixelSizeMatched?: boolean;
}

export interface CvTextPayloadResponse {
  /** 文本结果（如 Base64、JSON 等）。 */
  output?: string;
}

export interface InputOkResponse {
  /** 输入动作是否成功。 */
  ok?: boolean;
}

export interface InputMouseGetPositionResponse {
  /** 光标 X 坐标。 */
  x?: number;
  /** 光标 Y 坐标。 */
  y?: number;
}

export interface InputMouseMoveToRequest {
  /** 目标 X 坐标。 */
  x?: number;
  /** 目标 Y 坐标。 */
  y?: number;
}

export interface InputMouseMoveByRequest {
  /** X 方向增量。 */
  deltaX?: number;
  /** Y 方向增量。 */
  deltaY?: number;
}

export interface InputMouseClickRequest {
  /** 是否使用自定义 X 坐标。 */
  hasX?: boolean;
  /** 点击 X 坐标。 */
  x?: number;
  /** 是否使用自定义 Y 坐标。 */
  hasY?: boolean;
  /** 点击 Y 坐标。 */
  y?: number;
  /** 鼠标按钮名称。 */
  button?: string;
  /** 点击次数。 */
  clickCount?: number;
}

export interface InputMouseDownRequest {
  /** 是否使用自定义 X 坐标。 */
  hasX?: boolean;
  /** 按下时 X 坐标。 */
  x?: number;
  /** 是否使用自定义 Y 坐标。 */
  hasY?: boolean;
  /** 按下时 Y 坐标。 */
  y?: number;
  /** 鼠标按钮名称。 */
  button?: string;
  /** 修饰键集合。 */
  modifiers?: string[];
}

export interface InputMouseUpRequest {
  /** 鼠标按钮名称。 */
  button?: string;
  /** 修饰键集合。 */
  modifiers?: string[];
}

export interface InputMouseDragToRequest {
  /** 起点 X 坐标。 */
  startX?: number;
  /** 起点 Y 坐标。 */
  startY?: number;
  /** 终点 X 坐标。 */
  endX?: number;
  /** 终点 Y 坐标。 */
  endY?: number;
  /** 拖拽按钮名称。 */
  button?: string;
  /** 修饰键集合。 */
  modifiers?: string[];
}

export interface InputMouseDragByDistanceRequest {
  /** 起点 X 坐标。 */
  startX?: number;
  /** 起点 Y 坐标。 */
  startY?: number;
  /** X 方向拖拽距离。 */
  distanceX?: number;
  /** Y 方向拖拽距离。 */
  distanceY?: number;
  /** 拖拽按钮名称。 */
  button?: string;
  /** 修饰键集合。 */
  modifiers?: string[];
}

export interface InputMouseScrollRequest {
  /** 滚动触发点 X 坐标。 */
  x?: number;
  /** 滚动触发点 Y 坐标。 */
  y?: number;
  /** 滚轮行数（wheel lines）。 */
  wheelLines?: number;
}

export interface InputMouseHorizontalScrollRequest {
  /** 横向滚动触发点 X 坐标。 */
  x?: number;
  /** 横向滚动触发点 Y 坐标。 */
  y?: number;
  /** 横向滚轮行数。 */
  wheelLines?: number;
}

export interface InputMouseMoveSettingsResponse {
  /** 鼠标速度（每毫秒像素数）。 */
  movePixelsPerMillisecond?: number;
  /** 每步移动像素数。 */
  movePixelsPerStep?: number;
}

export interface InputMouseMoveSettingsRequest {
  /** 是否启用 `movePixelsPerMillisecond`。 */
  hasMovePixelsPerMillisecond?: boolean;
  /** 鼠标速度（每毫秒像素数）。 */
  movePixelsPerMillisecond?: number;
  /** 是否启用 `movePixelsPerStep`。 */
  hasMovePixelsPerStep?: boolean;
  /** 每步移动像素数。 */
  movePixelsPerStep?: number;
}

export interface InputKeyboardTypeTextRequest {
  /** 要输入的完整文本。 */
  text?: string;
}

export interface InputKeyboardTypeCharRequest {
  /** 单个字符。 */
  character?: string;
}

export interface InputKeyboardTypeKeysRequest {
  /** 按键序列。 */
  keys?: string[];
}

export interface InputKeyboardTypeSimultaneouslyRequest {
  /** 同时按下的按键集合。 */
  keys?: string[];
}

export interface InputKeyboardTypeKeyRequest {
  /** 单个按键名称。 */
  key?: string;
}

export interface InputKeyboardPressRequest {
  /** 按下的按键名称。 */
  key?: string;
}

export interface InputKeyboardReleaseRequest {
  /** 释放的按键名称。 */
  key?: string;
}

export interface InputKeyboardTypeSequenceRequest {
  /** 序列语法字符串。 */
  sequence?: string;
}

export interface InputWaitRequest {
  /** 等待时长（毫秒）。 */
  milliseconds?: number;
}

export interface OcrScreenStringsRequest {
  /** 屏幕识别区域。 */
  roi?: Roi;
  /** OCR engine（识别引擎）名称。 */
  engine?: string;
  /** 语言代码。 */
  language?: string;
  /** Paddle 中文模型标识。 */
  paddleChineseModel?: string;
}

export interface OcrScreenMatchRequest {
  /** 屏幕识别区域。 */
  roi?: Roi;
  /** 目标匹配文本。 */
  matchText?: string;
  /** OCR engine（识别引擎）名称。 */
  engine?: string;
  /** 语言代码。 */
  language?: string;
  /** Paddle 中文模型标识。 */
  paddleChineseModel?: string;
}

export interface OcrScreenTableRequest {
  /** 屏幕识别区域。 */
  roi?: Roi;
  /** OCR engine（识别引擎）名称。 */
  engine?: string;
  /** 语言代码。 */
  language?: string;
  /** Paddle 中文模型标识。 */
  paddleChineseModel?: string;
}

export interface OcrImageStringsRequest {
  /** 图片内容（路径或 Base64）。 */
  image?: string;
  /** OCR engine（识别引擎）名称。 */
  engine?: string;
  /** 语言代码。 */
  language?: string;
  /** Paddle 中文模型标识。 */
  paddleChineseModel?: string;
}

export interface OcrImageTableRequest {
  /** 图片内容（路径或 Base64）。 */
  image?: string;
  /** OCR engine（识别引擎）名称。 */
  engine?: string;
  /** 语言代码。 */
  language?: string;
  /** Paddle 中文模型标识。 */
  paddleChineseModel?: string;
}

export interface OcrScreenQuickTableRequest {
  /** 屏幕识别区域。 */
  roi?: Roi;
  /** 语言代码。 */
  language?: string;
  /** 是否开启 debug（调试）模式。 */
  debug?: boolean;
  /** 是否保存单元格调试图。 */
  saveCellDebugImages?: boolean;
  /** 调试输出目录。 */
  debugOutputDirectory?: string;
  /** 调试图文件名前缀。 */
  debugImageBaseName?: string;
}

export interface OcrImageQuickTableRequest {
  /** 图片内容（路径或 Base64）。 */
  image?: string;
  /** 语言代码。 */
  language?: string;
  /** 是否开启 debug（调试）模式。 */
  debug?: boolean;
  /** 是否保存单元格调试图。 */
  saveCellDebugImages?: boolean;
  /** 调试输出目录。 */
  debugOutputDirectory?: string;
  /** 调试图文件名前缀。 */
  debugImageBaseName?: string;
}

export interface OcrStringLineItem {
  /** 识别到的文本。 */
  text?: string;
  /** 边界框左上角 X。 */
  left?: number;
  /** 边界框左上角 Y。 */
  top?: number;
  /** 边界框宽度。 */
  width?: number;
  /** 边界框高度。 */
  height?: number;
  /** 中心点 X。 */
  centerX?: number;
  /** 中心点 Y。 */
  centerY?: number;
  /** 置信度。 */
  confidence?: number;
}

export interface OcrStringsResponse {
  /** 文本行结果列表。 */
  items?: OcrStringLineItem[];
}

export interface OcrTableCellItem {
  /** 单元格文本。 */
  text?: string;
  /** 单元格左上角 X。 */
  left?: number;
  /** 单元格左上角 Y。 */
  top?: number;
  /** 单元格宽度。 */
  width?: number;
  /** 单元格高度。 */
  height?: number;
  /** 行号。 */
  row?: number;
  /** 列号。 */
  column?: number;
}

export interface OcrTableResponse {
  /** 表格单元格集合。 */
  cells?: OcrTableCellItem[];
}

export interface ReadTextRequest {
  /** 文件路径。 */
  path?: string;
  /** 文本编码，如 `utf8`。 */
  encoding?: string;
}

export interface ReadTextResponse {
  /** 文件文本内容。 */
  text?: string;
}

export interface WriteTextRequest {
  /** 文件路径。 */
  path?: string;
  /** 写入文本内容。 */
  text?: string;
  /** 文本编码。 */
  encoding?: string;
}

export interface WriteTextResponse {
  /** 写入是否成功。 */
  success?: boolean;
}

export interface AppendTextRequest {
  /** 文件路径。 */
  path?: string;
  /** 追加文本内容。 */
  text?: string;
  /** 文本编码。 */
  encoding?: string;
}

export interface AppendTextResponse {
  /** 追加是否成功。 */
  success?: boolean;
}

export interface CopyFileRequest {
  /** 源文件路径。 */
  sourcePath?: string;
  /** 目标文件路径。 */
  targetPath?: string;
  /** 目标存在时是否覆盖。 */
  overwrite?: boolean;
}

export interface CopyFileResponse {
  /** 复制是否成功。 */
  success?: boolean;
}

export interface MoveFileRequest {
  /** 源文件路径。 */
  sourcePath?: string;
  /** 目标文件路径。 */
  targetPath?: string;
  /** 目标存在时是否覆盖。 */
  overwrite?: boolean;
}

export interface MoveFileResponse {
  /** 移动是否成功。 */
  success?: boolean;
}

export interface DeleteFileRequest {
  /** 待删除文件路径。 */
  path?: string;
}

export interface DeleteFileResponse {
  /** 删除是否成功。 */
  success?: boolean;
}

export interface ExistsRequest {
  /** 待检查路径。 */
  path?: string;
}

export interface ExistsResponse {
  /** 路径是否存在。 */
  exists?: boolean;
}

export interface GetItemInfoRequest {
  /** 待查询路径。 */
  path?: string;
}

export interface GetItemInfoResponse {
  /** 文件名或目录名。 */
  name?: string;
  /** 绝对路径。 */
  fullPath?: string;
  /** 是否目录。 */
  isDirectory?: boolean;
  /** 大小（字节）。 */
  sizeBytes?: string | number;
  /** 是否包含 `sizeBytes` 字段。 */
  hasSizeBytes?: boolean;
  /** 创建时间（UTC）。 */
  creationTimeUtc?: ProtoTimestamp;
  /** 最后写入时间（UTC）。 */
  lastWriteTimeUtc?: ProtoTimestamp;
  /** 最后访问时间（UTC）。 */
  lastAccessTimeUtc?: ProtoTimestamp;
  /** 文件属性文本。 */
  attributes?: string;
}

export interface CreateDirectoryRequest {
  /** 目录路径。 */
  path?: string;
}

export interface CreateDirectoryResponse {
  /** 创建是否成功。 */
  success?: boolean;
}

export interface DeleteDirectoryRequest {
  /** 目录路径。 */
  path?: string;
}

export interface DeleteDirectoryResponse {
  /** 删除是否成功。 */
  success?: boolean;
}

export interface ListDirectoryRequest {
  /** 目录路径。 */
  path?: string;
  /** 文件匹配 pattern（模式）。 */
  pattern?: string;
}

export interface ListDirectoryResponse {
  /** 子项名称集合。 */
  names?: string[];
}

export interface MoveDirectoryRequest {
  /** 源目录路径。 */
  sourcePath?: string;
  /** 目标目录路径。 */
  targetPath?: string;
  /** 目标存在时是否覆盖。 */
  overwrite?: boolean;
}

export interface MoveDirectoryResponse {
  /** 移动是否成功。 */
  success?: boolean;
}

export interface SearchFilesRequest {
  /** 搜索根目录。 */
  rootPath?: string;
  /** 文件名匹配 pattern（模式）。 */
  pattern?: string;
}

export interface SearchFilesResponse {
  /** 命中的完整路径列表。 */
  paths?: string[];
}

export interface FindExeShortcutsRequest {
  /** 可执行文件名，如 `notepad.exe`。 */
  exeName?: string;
}

export interface FindExeShortcutsResponse {
  /** 快捷方式目标路径集合。 */
  targetPaths?: string[];
}

export interface CaptureCreateListenWindowRequest {
  /** 数据存储目录。 */
  storageDirectory?: string;
  /** 代理监听端口。 */
  proxyListenPort?: number;
  /** 单个分片最大 Body 字节数。 */
  maxBodyBytesPerPart?: number;
  /** 刷盘间隔（毫秒）。 */
  flushIntervalMs?: number;
  /** 单批最大行数。 */
  flushBatchMaxRows?: number;
  /** 单批最大字节数。 */
  flushBatchMaxBytes?: string | number;
  /** 是否允许全部流量。 */
  trafficAllowAll?: boolean;
  /** 流量主机名包含白名单。 */
  trafficHostContains?: string[];
  /** 流量路径前缀白名单。 */
  trafficPathPrefixes?: string[];
  /** 是否写入当前用户证书信任。 */
  mitmUserTrustRoot?: boolean;
  /** 是否写入本机证书信任。 */
  mitmMachineTrustRoot?: boolean;
  /** 是否以管理员权限信任根证书。 */
  mitmTrustRootAsAdministrator?: boolean;
  /** 是否启用通知事件。 */
  enableNotifications?: boolean;
  /** 通知去抖间隔（毫秒）。 */
  notificationDebounceMs?: number;
  /** 通知进程名包含过滤。 */
  notificationProcessNameContains?: string[];
  /** 通知窗口标题包含过滤。 */
  notificationTitleContains?: string;
  /** 窗口 Hook 事件类型集合。 */
  hookWindowEventTypes?: string[];
  /** 是否启用 ETW 探针。 */
  enableEtwProbe?: boolean;
  /** ETW provider 名称集合。 */
  etwProviderNames?: string[];
  /** ETW 窗口 provider 名称集合。 */
  etwWindowProviderNames?: string[];
  /** ETW 窗口事件类型集合。 */
  etwWindowEventTypes?: string[];
  /** ETW 关键字掩码（match any keyword）。 */
  etwMatchAnyKeyword?: string | number;
  /** 是否启用 `etwMatchAnyKeyword`。 */
  hasEtwMatchAnyKeyword?: boolean;
  /** ETW 级别（level）。 */
  etwLevel?: number;
  /** 是否启用 `etwLevel`。 */
  hasEtwLevel?: boolean;
  /** ETW 队列容量。 */
  etwQueueCapacity?: number;
  /** ETW 窗口队列容量。 */
  etwWindowQueueCapacity?: number;
  /** 流量统计间隔（毫秒）。 */
  trafficStatsIntervalMs?: number;
}

export interface CaptureCreateListenWindowResponse {
  /** 监听窗口 ID。 */
  listenWindowId?: string;
  /** SQLite 落盘路径。 */
  sqlitePath?: string;
  /** 实际监听端口。 */
  proxyListenPort?: number;
}

export interface CaptureStopListenWindowRequest {
  /** 监听窗口 ID。 */
  listenWindowId?: string;
}

export interface CaptureStopListenWindowResponse {
  /** 是否已停止。 */
  stopped?: boolean;
  /** SQLite 落盘路径。 */
  sqlitePath?: string;
}

export interface CaptureHistoryQueryRequest {
  /** 监听窗口 ID。 */
  listenWindowId?: string;
  /** 查询条数上限。 */
  limit?: number;
  /** 分页偏移量。 */
  offset?: number;
  /** 仅查询该时间之前数据（UTC 字符串）。 */
  beforeCapturedAtUtc?: string;
}

export interface CaptureHttpExchangeItem {
  /** 记录 ID。 */
  id?: string | number;
  /** 抓取时间。 */
  capturedAt?: string;
  /** HTTP 方法。 */
  method?: string;
  /** URL 协议，如 `http`/`https`。 */
  scheme?: string;
  /** 主机名。 */
  host?: string;
  /** 端口。 */
  port?: number;
  /** 请求路径。 */
  path?: string;
  /** Query 字符串。 */
  queryText?: string;
  /** 展示用完整 URL。 */
  urlDisplay?: string;
  /** 响应状态码。 */
  responseStatus?: number;
  /** 是否有响应状态码。 */
  hasResponseStatus?: boolean;
  /** 请求耗时（毫秒）。 */
  durationMs?: number;
  /** 是否有耗时数据。 */
  hasDurationMs?: boolean;
  /** 错误文本。 */
  errorText?: string;
  /** 客户端进程 ID。 */
  clientProcessId?: number;
  /** 是否有客户端进程 ID。 */
  hasClientProcessId?: boolean;
  /** 客户端进程名。 */
  clientProcessName?: string;
}

export interface CaptureHistoryQueryResponse {
  /** 抓包记录集合。 */
  items?: CaptureHttpExchangeItem[];
}

export interface CaptureNotificationEvent {
  /** 通知 JSON 载荷。 */
  jsonPayload?: string;
}

export interface TrafficChunk {
  /** 流量分片 JSON 载荷。 */
  jsonPayload?: string;
}

export interface FlaUiOkResponse {
  /** 执行是否成功。 */
  ok?: boolean;
}

export interface SessionIdRequest {
  /** FlaUI 会话 ID。 */
  sessionId?: string;
}

export interface SessionElementRequest {
  /** FlaUI 会话 ID。 */
  sessionId?: string;
  /** 元素 ID。 */
  elementId?: string;
}

export interface SessionCreateRequest {
  /** 自动化类型（Automation Type）。 */
  automationType?: string;
  /** 应用可执行文件路径。 */
  executablePath?: string;
  /** 启动参数。 */
  arguments?: string;
  /** 进程不存在时是否自动启动。 */
  launchIfNotRunning?: boolean;
  /** 多进程场景下目标索引。 */
  processIndex?: number;
}

export interface SessionCreateResponse {
  /** 新建会话 ID。 */
  sessionId?: string;
  /** 会话关联进程 ID。 */
  processId?: number;
  /** 实际自动化类型。 */
  automationType?: string;
}

export interface SessionCloseApplicationRequest {
  /** 会话 ID。 */
  sessionId?: string;
  /** 关闭失败时是否强制结束。 */
  killIfCloseFails?: boolean;
}

export interface FlaUiCloseApplicationResponse {
  /** 调用是否成功。 */
  ok?: boolean;
  /** 应用是否已关闭。 */
  closed?: boolean;
}

export interface WaitTimeoutPayload {
  /** 是否启用 `timeoutMs`。 */
  hasTimeoutMs?: boolean;
  /** 超时毫秒数。 */
  timeoutMs?: number;
}

export interface SessionWaitRequest {
  /** 会话 ID。 */
  sessionId?: string;
  /** 超时配置。 */
  timeout?: WaitTimeoutPayload;
}

export interface FlaUiWaitBusyResponse {
  /** 调用是否成功。 */
  ok?: boolean;
  /** 等待结果。 */
  result?: boolean;
}

export interface FlaUiWaitHandleResponse {
  /** 调用是否成功。 */
  ok?: boolean;
  /** 等待结果。 */
  result?: boolean;
}

export interface FindElementPayload {
  /** 根节点类型。 */
  rootKind?: string;
  /** 根元素 ID。 */
  rootElementId?: string;
  /** 查找范围（scope）。 */
  scope?: string;
  /** 自动化 ID 条件。 */
  automationId?: string;
  /** 名称条件。 */
  name?: string;
  /** 类名条件。 */
  className?: string;
  /** 控件类型条件。 */
  controlType?: string;
  /** XPath 条件。 */
  xpath?: string;
  /** 是否启用主窗口等待超时。 */
  hasMainWindowWaitTimeoutMs?: boolean;
  /** 主窗口等待超时（毫秒）。 */
  mainWindowWaitTimeoutMs?: number;
}

export interface FindByXPathPayload {
  /** 根节点类型。 */
  rootKind?: string;
  /** 根元素 ID。 */
  rootElementId?: string;
  /** XPath 表达式。 */
  xpath?: string;
  /** 是否启用主窗口等待超时。 */
  hasMainWindowWaitTimeoutMs?: boolean;
  /** 主窗口等待超时（毫秒）。 */
  mainWindowWaitTimeoutMs?: number;
}

export interface SessionFindElementRequest {
  /** 会话 ID。 */
  sessionId?: string;
  /** 查找条件。 */
  find?: FindElementPayload;
}

export interface SessionFindByXPathRequest {
  /** 会话 ID。 */
  sessionId?: string;
  /** XPath 查找条件。 */
  find?: FindByXPathPayload;
}

export interface RectDto {
  /** 矩形左上角 X。 */
  x?: number;
  /** 矩形左上角 Y。 */
  y?: number;
  /** 宽度。 */
  width?: number;
  /** 高度。 */
  height?: number;
}

export interface ElementInfoResponse {
  /** 元素 ID。 */
  elementId?: string;
  /** 元素名称。 */
  name?: string;
  /** 自动化 ID。 */
  automationId?: string;
  /** 类名。 */
  className?: string;
  /** 控件类型。 */
  controlType?: string;
  /** UI 框架类型。 */
  frameworkType?: string;
  /** 是否可用。 */
  isEnabled?: boolean;
  /** 是否离屏。 */
  isOffscreen?: boolean;
  /** 是否仍可访问。 */
  isAvailable?: boolean;
  /** 边界矩形。 */
  bounds?: RectDto;
}

export interface ElementRefResponse {
  /** 元素 ID。 */
  elementId?: string;
}

export interface ElementRefListResponse {
  /** 元素引用列表。 */
  items?: ElementRefResponse[];
}

export interface ClickPayload {
  /** 是否启用 `moveMouse`。 */
  hasMoveMouse?: boolean;
  /** 点击前是否移动鼠标到元素。 */
  moveMouse?: boolean;
}

export interface SessionElementClickRequest {
  /** 会话 ID。 */
  sessionId?: string;
  /** 元素 ID。 */
  elementId?: string;
  /** 是否启用 `click` 参数。 */
  hasClick?: boolean;
  /** 点击参数。 */
  click?: ClickPayload;
}
