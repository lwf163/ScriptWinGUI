import type * as grpc from '@grpc/grpc-js';
import { promisifyUnary } from '../grpc-call.js';
import type {
  OcrImageQuickTableRequest,
  OcrImageStringsRequest,
  OcrImageTableRequest,
  OcrScreenMatchRequest,
  OcrScreenQuickTableRequest,
  OcrScreenStringsRequest,
  OcrScreenTableRequest,
  OcrStringsResponse,
  OcrTableResponse,
} from '../proto-types.js';

/**
 * OCR（光学字符识别）gRPC 客户端（Proto 服务 `swg.ocr.OcrService`）。
 *
 * 提供屏幕/图像文字识别、表格识别、带匹配的文本识别等功能。
 * 支持 PaddleSharp 和 Tesseract 两种识别引擎，以及中文和英文两种语言。
 */
export class OcrServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /**
   * 对屏幕指定区域进行 OCR 文字识别，返回所有识别到的文本行。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕识别区域（left/top/width/height）
   *   - `engine`（string，可选）：识别引擎，`PaddleSharp`（默认）或 `Tesseract`
   *   - `language`（string，可选）：识别语言，`Chinese`（默认）或 `English`
   *   - `paddleChineseModel`（string，可选）：PaddleSharp 中文模型版本，`V3`（默认）/`V4`/`V5`
   * @returns 包含 `items`（数组），每条记录含：
   *   - `text`（string）：识别到的文本内容
   *   - `left`/`top`/`width`/`height`（number）：文本行区域坐标
   *   - `centerX`/`centerY`（number）：文本行中心点坐标
   *   - `confidence`（number）：识别置信度（0.0-1.0）
   * @throws gRPC `InvalidArgument` — roi 未提供/尺寸非法，或引擎/语言值无效
   */
  recognizeScreenStrings(request: OcrScreenStringsRequest): Promise<OcrStringsResponse> {
    return promisifyUnary<OcrScreenStringsRequest, OcrStringsResponse>(this.client, 'recognizeScreenStrings', request);
  }

  /**
   * 对屏幕指定区域进行 OCR 文字识别，仅返回包含指定匹配文本的行。
   * 服务端不对 `matchText` 执行 Trim 操作，需调用方自行处理。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕识别区域
   *   - `matchText`（string，必填）：要匹配的文本，不能为空字符串
   *   - `engine`/`language`/`paddleChineseModel`：同 `recognizeScreenStrings`
   * @returns 仅包含匹配的文本行，字段同 `recognizeScreenStrings`
   * @throws gRPC `InvalidArgument` — matchText 为 null 或空字符串，或 roi 非法
   */
  recognizeScreenStringsMatch(request: OcrScreenMatchRequest): Promise<OcrStringsResponse> {
    return promisifyUnary<OcrScreenMatchRequest, OcrStringsResponse>(
      this.client,
      'recognizeScreenStringsMatch',
      request
    );
  }

  /**
   * 对屏幕指定区域进行表格结构识别，返回单元格信息。
   * 仅支持 `engine=PaddleSharp`，使用 PaddleSharp 内置的表格识别模型。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕识别区域
   *   - `engine`（string，可选）：必须为 `PaddleSharp`
   *   - `language`/`paddleChineseModel`：同 `recognizeScreenStrings`
   * @returns 包含 `cells`（数组），每个单元格含：
   *   - `text`（string）：单元格文本
   *   - `left`/`top`/`width`/`height`（number）：单元格区域坐标
   *   - `row`/`column`（number）：单元格行列索引
   * @throws gRPC `InvalidArgument` — roi 非法，或引擎非 PaddleSharp
   */
  recognizeScreenTable(request: OcrScreenTableRequest): Promise<OcrTableResponse> {
    return promisifyUnary<OcrScreenTableRequest, OcrTableResponse>(this.client, 'recognizeScreenTable', request);
  }

  /**
   * 对图像文件进行 OCR 文字识别。
   * 图像可通过文件路径、Base64 编码或 Data URL 提供。
   *
   * @param request 请求参数：
   *   - `image`（string，必填）：图像路径或 Base64 / Data URL 编码
   *   - `engine`/`language`/`paddleChineseModel`：同 `recognizeScreenStrings`
   * @returns 字段同 `recognizeScreenStrings`
   * @throws gRPC `InvalidArgument` — image 为空
   */
  recognizeImageStrings(request: OcrImageStringsRequest): Promise<OcrStringsResponse> {
    return promisifyUnary<OcrImageStringsRequest, OcrStringsResponse>(this.client, 'recognizeImageStrings', request);
  }

  /**
   * 对图像文件进行表格结构识别。
   * 仅支持 `engine=PaddleSharp`。
   *
   * @param request 请求参数：
   *   - `image`（string，必填）：图像路径或 Base64 / Data URL 编码
   *   - `engine`/`language`/`paddleChineseModel`：同 `recognizeScreenTable`
   * @returns 字段同 `recognizeScreenTable`
   * @throws gRPC `InvalidArgument` — image 为空，或引擎非 PaddleSharp
   */
  recognizeImageTable(request: OcrImageTableRequest): Promise<OcrTableResponse> {
    return promisifyUnary<OcrImageTableRequest, OcrTableResponse>(this.client, 'recognizeImageTable', request);
  }

  /**
   * 对屏幕指定区域使用 QuickTable 算法进行快速表格检测与识别。
   * QuickTable 基于线条检测和交叉点分析实现表格结构识别，适用于规则线条表格，速度较快。
   *
   * @param request 请求参数：
   *   - `roi`（必填）：屏幕识别区域
   *   - `language`（string，可选）：`Chinese`（默认）或 `English`
   *   - `debug`（boolean）：是否启用调试输出
   *   - `saveCellDebugImages`（boolean）：是否保存单元格调试图像
   *   - `debugOutputDirectory`（string，可选）：调试图像输出目录
   *   - `debugImageBaseName`（string，可选）：调试图像文件名前缀（默认 `quicktable`）
   * @returns 字段同 `recognizeScreenTable`
   * @throws gRPC `InvalidArgument` — roi 非法
   */
  recognizeScreenQuickTable(request: OcrScreenQuickTableRequest): Promise<OcrTableResponse> {
    return promisifyUnary<OcrScreenQuickTableRequest, OcrTableResponse>(
      this.client,
      'recognizeScreenQuickTable',
      request
    );
  }

  /**
   * 对图像文件使用 QuickTable 算法进行快速表格检测与识别。
   *
   * @param request 请求参数：
   *   - `image`（string，必填）：图像路径或 Base64 / Data URL 编码
   *   - 其余参数同 `recognizeScreenQuickTable`
   * @returns 字段同 `recognizeScreenTable`
   * @throws gRPC `InvalidArgument` — image 为空
   */
  recognizeImageQuickTable(request: OcrImageQuickTableRequest): Promise<OcrTableResponse> {
    return promisifyUnary<OcrImageQuickTableRequest, OcrTableResponse>(
      this.client,
      'recognizeImageQuickTable',
      request
    );
  }
}
