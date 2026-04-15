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
 * `OcrService` 客户端（包 `swg.ocr`）。
 */
export class OcrServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /** 识别屏幕文本行。 */
  recognizeScreenStrings(request: OcrScreenStringsRequest): Promise<OcrStringsResponse> {
    return promisifyUnary<OcrScreenStringsRequest, OcrStringsResponse>(this.client, 'recognizeScreenStrings', request);
  }
  /** 识别屏幕文本并按关键字过滤。 */
  recognizeScreenStringsMatch(request: OcrScreenMatchRequest): Promise<OcrStringsResponse> {
    return promisifyUnary<OcrScreenMatchRequest, OcrStringsResponse>(
      this.client,
      'recognizeScreenStringsMatch',
      request
    );
  }
  /** 识别屏幕表格。 */
  recognizeScreenTable(request: OcrScreenTableRequest): Promise<OcrTableResponse> {
    return promisifyUnary<OcrScreenTableRequest, OcrTableResponse>(this.client, 'recognizeScreenTable', request);
  }
  /** 识别图片文本行。 */
  recognizeImageStrings(request: OcrImageStringsRequest): Promise<OcrStringsResponse> {
    return promisifyUnary<OcrImageStringsRequest, OcrStringsResponse>(this.client, 'recognizeImageStrings', request);
  }
  /** 识别图片表格。 */
  recognizeImageTable(request: OcrImageTableRequest): Promise<OcrTableResponse> {
    return promisifyUnary<OcrImageTableRequest, OcrTableResponse>(this.client, 'recognizeImageTable', request);
  }
  /** 使用快速模式识别屏幕表格。 */
  recognizeScreenQuickTable(request: OcrScreenQuickTableRequest): Promise<OcrTableResponse> {
    return promisifyUnary<OcrScreenQuickTableRequest, OcrTableResponse>(
      this.client,
      'recognizeScreenQuickTable',
      request
    );
  }
  /** 使用快速模式识别图片表格。 */
  recognizeImageQuickTable(request: OcrImageQuickTableRequest): Promise<OcrTableResponse> {
    return promisifyUnary<OcrImageQuickTableRequest, OcrTableResponse>(
      this.client,
      'recognizeImageQuickTable',
      request
    );
  }
}
