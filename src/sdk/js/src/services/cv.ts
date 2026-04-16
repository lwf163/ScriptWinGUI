import type * as grpc from '@grpc/grpc-js';
import { promisifyUnary } from '../grpc-call.js';
import type {
  CaptureFullScreenRequest,
  CaptureRegionRequest,
  CountPixelsRgbRequest,
  CountPixelsRgbResponse,
  CvTextPayloadResponse,
  FindAllTemplatesRequest,
  FindAllTemplatesResponse,
  FindOneOfTemplatesRequest,
  FindOneOfTemplatesResponse,
  FindPixelsHsvRequest,
  FindPixelsRgbMultipleRequest,
  FindPixelsRgbRequest,
  FindSingleTemplateRequest,
  FindSingleTemplateResponse,
  GetPixelRgbRequest,
  PixelRgbResponse,
  ScreenPointsResponse,
  WindowRoiConsistencyRequest,
  WindowRoiConsistencyResponse,
} from '../proto-types.js';

/**
 * CV（计算机视觉）gRPC 客户端（Proto 服务 `swg.cv.CvService`）。
 *
 * 提供屏幕模板匹配、像素查找/计数、屏幕截图捕获、窗口 ROI 一致性检测等功能。
 * 所有方法均为无状态调用，线程安全。
 */
export class CvServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /**
   * 在指定屏幕区域内查找单个模板图像的最佳匹配位置。
   * 使用 OpenCV 模板匹配算法，在 ROI 对应的屏幕截图中搜索与模板最相似的区域。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕搜索区域，含 `left`/`top`/`width`/`height`（width/height 必须为正）
   *   - `template`（string，必填）：模板图像，支持文件路径或 Base64 编码
   *   - `threshold`（number）：匹配阈值（0.0-1.0），低于此值视为未找到
   * @returns 包含以下字段：
   *   - `found`（boolean）：是否找到匹配
   *   - `score`（number）：匹配得分（0.0-1.0，越高越相似）
   *   - `left`/`top`/`width`/`height`（number）：匹配区域在屏幕上的绝对坐标
   * @throws gRPC `InvalidArgument` — template 为空、roi 未提供或尺寸非法
   */
  findSingleTemplate(request: FindSingleTemplateRequest): Promise<FindSingleTemplateResponse> {
    return promisifyUnary<FindSingleTemplateRequest, FindSingleTemplateResponse>(
      this.client,
      'findSingleTemplate',
      request
    );
  }

  /**
   * 在指定屏幕区域内从一组候选模板中查找最先（或最优）匹配的模板。
   * 依次对每个模板执行匹配，根据 `preference` 策略返回结果：
   * `FirstQualified`（默认）返回首个超过阈值的匹配，`BestScore` 返回得分最高的匹配。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕搜索区域
   *   - `templates`（string[]，必填）：候选模板图像列表，不可为空
   *   - `threshold`（number）：匹配阈值
   *   - `preference`（string，可选）：匹配策略，`FirstQualified`（默认）或 `BestScore`
   * @returns 包含以下字段：
   *   - `found`（boolean）：是否找到匹配
   *   - `templateIndex`（number）：匹配到的模板在列表中的索引
   *   - `score`（number）：匹配得分
   *   - `left`/`top`/`width`/`height`（number）：匹配区域坐标
   * @throws gRPC `InvalidArgument` — templates 为空、preference 值非法或 roi 非法
   */
  findOneOfTemplates(request: FindOneOfTemplatesRequest): Promise<FindOneOfTemplatesResponse> {
    return promisifyUnary<FindOneOfTemplatesRequest, FindOneOfTemplatesResponse>(
      this.client,
      'findOneOfTemplates',
      request
    );
  }

  /**
   * 在指定屏幕区域内查找多个模板，确保每个模板至少匹配到一次。
   * 对模板列表中的每个模板分别执行匹配，即使某些模板未匹配到也会返回对应条目（found=false）。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕搜索区域
   *   - `templates`（string[]）：模板图像列表，为空时 allFound 为 true
   *   - `threshold`（number）：匹配阈值
   * @returns 包含以下字段：
   *   - `allFound`（boolean）：是否所有模板均匹配成功
   *   - `items`（数组）：每个模板的匹配结果，含 `templateIndex`/`found`/`score`/`left`/`top`/`width`/`height`
   */
  findEachTemplateAtLeastOnce(request: FindAllTemplatesRequest): Promise<FindAllTemplatesResponse> {
    return promisifyUnary<FindAllTemplatesRequest, FindAllTemplatesResponse>(
      this.client,
      'findEachTemplateAtLeastOnce',
      request
    );
  }

  /**
   * 在指定屏幕区域内查找与目标 RGB 颜色精确匹配的像素坐标。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕搜索区域
   *   - `rgb`（必填）：目标 RGB 颜色，含 `r`/`g`/`b`（各 0-255）
   * @returns 包含 `points`（数组）：匹配像素的绝对屏幕坐标列表，每个含 `x`/`y`
   * @throws gRPC `InvalidArgument` — rgb 或 roi 未提供
   */
  findPixelsRgb(request: FindPixelsRgbRequest): Promise<ScreenPointsResponse> {
    return promisifyUnary<FindPixelsRgbRequest, ScreenPointsResponse>(this.client, 'findPixelsRgb', request);
  }

  /**
   * 在指定屏幕区域内查找与任意一组 RGB 颜色匹配的像素坐标（多色查找）。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕搜索区域
   *   - `rgbColors`（数组，必填）：目标 RGB 颜色列表，不可为空
   * @returns 同 `findPixelsRgb`，返回匹配像素坐标列表
   * @throws gRPC `InvalidArgument` — rgbColors 为空或 roi 非法
   */
  findPixelsRgbMultiple(request: FindPixelsRgbMultipleRequest): Promise<ScreenPointsResponse> {
    return promisifyUnary<FindPixelsRgbMultipleRequest, ScreenPointsResponse>(
      this.client,
      'findPixelsRgbMultiple',
      request
    );
  }

  /**
   * 在指定屏幕区域内查找与目标 HSV 颜色匹配的像素坐标。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕搜索区域
   *   - `hsv`（必填）：目标 HSV 颜色，含 `h`/`s`/`v`（各为 number）
   * @returns 同 `findPixelsRgb`，返回匹配像素坐标列表
   * @throws gRPC `InvalidArgument` — hsv 或 roi 未提供
   */
  findPixelsHsv(request: FindPixelsHsvRequest): Promise<ScreenPointsResponse> {
    return promisifyUnary<FindPixelsHsvRequest, ScreenPointsResponse>(this.client, 'findPixelsHsv', request);
  }

  /**
   * 统计指定屏幕区域内与目标 RGB 颜色匹配的像素数量。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕搜索区域
   *   - `rgb`（必填）：目标 RGB 颜色
   * @returns 包含 `count`（number）：匹配像素数量
   * @throws gRPC `InvalidArgument` — rgb 或 roi 未提供
   */
  countPixelsRgb(request: CountPixelsRgbRequest): Promise<CountPixelsRgbResponse> {
    return promisifyUnary<CountPixelsRgbRequest, CountPixelsRgbResponse>(this.client, 'countPixelsRgb', request);
  }

  /**
   * 获取屏幕上指定坐标处的像素 RGB 颜色值。
   *
   * @param request 请求参数：
   *   - `x`（number）：像素 X 坐标（屏幕绝对坐标）
   *   - `y`（number）：像素 Y 坐标（屏幕绝对坐标）
   * @returns 包含以下字段：
   *   - `success`（boolean）：是否成功获取（坐标越界时为 false）
   *   - `r`/`g`/`b`（number）：像素颜色分量（仅 success 为 true 时有效）
   */
  getPixelRgb(request: GetPixelRgbRequest): Promise<PixelRgbResponse> {
    return promisifyUnary<GetPixelRgbRequest, PixelRgbResponse>(this.client, 'getPixelRgb', request);
  }

  /**
   * 检查指定窗口的实际尺寸与其 ROI 截图的一致性（DPI 感知）。
   * 用于诊断高 DPI 环境下窗口截图尺寸与逻辑尺寸不匹配的问题。
   *
   * @param request 请求参数：
   *   - `windowHandle`（string，必填）：窗口句柄（HWND），支持十进制或 0x 十六进制格式
   * @returns 包含以下字段：
   *   - `success`（boolean）：检测是否成功执行
   *   - `roi`：窗口的 ROI 信息（left/top/width/height）
   *   - `capturedWidth`/`capturedHeight`（number）：实际截取的像素尺寸
   *   - `windowDpi`（number）：窗口 DPI 值
   *   - `isPixelSizeMatched`（boolean）：像素尺寸是否匹配
   * @throws gRPC `InvalidArgument` — windowHandle 为空或格式无效
   */
  checkWindowRoiConsistency(request: WindowRoiConsistencyRequest): Promise<WindowRoiConsistencyResponse> {
    return promisifyUnary<WindowRoiConsistencyRequest, WindowRoiConsistencyResponse>(
      this.client,
      'checkWindowRoiConsistency',
      request
    );
  }

  /**
   * 截取全屏图像并按指定格式输出。
   *
   * @param request 请求参数：
   *   - `options`（可选）：截图选项，为空时使用默认值
   *     - `outputKind`（string，可选）：`Base64`（默认）或 `FilePath`
   *     - `targetFilePath`（string，条件必填）：outputKind 为 FilePath 时必填
   *     - `imageFormat`（string，可选）：`Png`（默认）、`Jpeg` 或 `Bmp`
   *     - `base64Variant`（string，可选）：`Raw`（默认）或 `DataUrl`
   *     - `jpegQuality`（number，可选）：JPEG 质量（0-100，默认 0 即使用引擎默认值）
   * @returns 包含 `output`（string）：Base64 编码字符串或保存的文件路径
   * @throws gRPC `InvalidArgument` — outputKind 为 FilePath 但 targetFilePath 为空，或枚举值无效
   */
  captureFullScreen(request: CaptureFullScreenRequest): Promise<CvTextPayloadResponse> {
    return promisifyUnary<CaptureFullScreenRequest, CvTextPayloadResponse>(this.client, 'captureFullScreen', request);
  }

  /**
   * 截取屏幕指定区域的图像并按指定格式输出。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：截取区域（left/top/width/height）
   *   - `options`（可选）：截图选项，详见 `captureFullScreen`
   * @returns 同 `captureFullScreen`，返回 `output`（Base64 或文件路径）
   * @throws gRPC `InvalidArgument` — roi 未提供/尺寸非法，或截图选项参数无效
   */
  captureRegion(request: CaptureRegionRequest): Promise<CvTextPayloadResponse> {
    return promisifyUnary<CaptureRegionRequest, CvTextPayloadResponse>(this.client, 'captureRegion', request);
  }
}
