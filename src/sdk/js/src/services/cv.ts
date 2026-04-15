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
 * `CvService` 客户端（包 `swg.cv`）。
 */
export class CvServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /** 单模板匹配。 */
  findSingleTemplate(request: FindSingleTemplateRequest): Promise<FindSingleTemplateResponse> {
    return promisifyUnary<FindSingleTemplateRequest, FindSingleTemplateResponse>(
      this.client,
      'findSingleTemplate',
      request
    );
  }
  /** 多模板择优匹配。 */
  findOneOfTemplates(request: FindOneOfTemplatesRequest): Promise<FindOneOfTemplatesResponse> {
    return promisifyUnary<FindOneOfTemplatesRequest, FindOneOfTemplatesResponse>(
      this.client,
      'findOneOfTemplates',
      request
    );
  }
  /** 每个模板至少命中一次。 */
  findEachTemplateAtLeastOnce(request: FindAllTemplatesRequest): Promise<FindAllTemplatesResponse> {
    return promisifyUnary<FindAllTemplatesRequest, FindAllTemplatesResponse>(
      this.client,
      'findEachTemplateAtLeastOnce',
      request
    );
  }
  /** 按 RGB 查找像素点。 */
  findPixelsRgb(request: FindPixelsRgbRequest): Promise<ScreenPointsResponse> {
    return promisifyUnary<FindPixelsRgbRequest, ScreenPointsResponse>(this.client, 'findPixelsRgb', request);
  }
  /** 按多个 RGB 条件查找像素点。 */
  findPixelsRgbMultiple(request: FindPixelsRgbMultipleRequest): Promise<ScreenPointsResponse> {
    return promisifyUnary<FindPixelsRgbMultipleRequest, ScreenPointsResponse>(
      this.client,
      'findPixelsRgbMultiple',
      request
    );
  }
  /** 按 HSV 查找像素点。 */
  findPixelsHsv(request: FindPixelsHsvRequest): Promise<ScreenPointsResponse> {
    return promisifyUnary<FindPixelsHsvRequest, ScreenPointsResponse>(this.client, 'findPixelsHsv', request);
  }
  /** 统计 RGB 像素数量。 */
  countPixelsRgb(request: CountPixelsRgbRequest): Promise<CountPixelsRgbResponse> {
    return promisifyUnary<CountPixelsRgbRequest, CountPixelsRgbResponse>(this.client, 'countPixelsRgb', request);
  }
  /** 获取单点 RGB 值。 */
  getPixelRgb(request: GetPixelRgbRequest): Promise<PixelRgbResponse> {
    return promisifyUnary<GetPixelRgbRequest, PixelRgbResponse>(this.client, 'getPixelRgb', request);
  }
  /** 校验窗口 ROI 一致性。 */
  checkWindowRoiConsistency(request: WindowRoiConsistencyRequest): Promise<WindowRoiConsistencyResponse> {
    return promisifyUnary<WindowRoiConsistencyRequest, WindowRoiConsistencyResponse>(
      this.client,
      'checkWindowRoiConsistency',
      request
    );
  }
  /** 全屏截图。 */
  captureFullScreen(request: CaptureFullScreenRequest): Promise<CvTextPayloadResponse> {
    return promisifyUnary<CaptureFullScreenRequest, CvTextPayloadResponse>(this.client, 'captureFullScreen', request);
  }
  /** 区域截图。 */
  captureRegion(request: CaptureRegionRequest): Promise<CvTextPayloadResponse> {
    return promisifyUnary<CaptureRegionRequest, CvTextPayloadResponse>(this.client, 'captureRegion', request);
  }
}
