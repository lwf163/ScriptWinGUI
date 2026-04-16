using Grpc.Core;
using Swg.Grpc.Api;
using Swg.Grpc.Cv;

namespace Swg.Grpc.Services;

/// <summary>
/// CV（计算机视觉）gRPC 服务实现：提供模板匹配、像素操作、屏幕截图等计算机视觉能力。
/// <para>
/// 继承自 <c>CvService.CvServiceBase</c>，由 gRPC 运行时自动注册。
/// 所有 RPC 均通过 <see cref="GrpcRouteRunner"/> 统一异常映射。
/// </para>
/// <para>对应 Proto 定义：<c>swg.cv.CvService</c></para>
/// </summary>
public sealed class CvGrpcService : CvService.CvServiceBase
{
    /// <summary>在指定 ROI 内查找单个模板的最佳匹配位置。</summary>
    public override Task<FindSingleTemplateResponse> FindSingleTemplate(FindSingleTemplateRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindSingleTemplate(request)));

    /// <summary>从一组候选模板中查找最先/最优匹配。</summary>
    public override Task<FindOneOfTemplatesResponse> FindOneOfTemplates(FindOneOfTemplatesRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindOneOfTemplates(request)));

    /// <summary>查找多个模板，确保每个模板至少匹配到一次。</summary>
    public override Task<FindAllTemplatesResponse> FindEachTemplateAtLeastOnce(FindAllTemplatesRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindEachTemplateAtLeastOnce(request)));

    /// <summary>在指定 ROI 内查找与目标 RGB 颜色匹配的像素坐标。</summary>
    public override Task<ScreenPointsResponse> FindPixelsRgb(FindPixelsRgbRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindPixelsRgb(request)));

    /// <summary>在指定 ROI 内查找与任意一组 RGB 颜色匹配的像素坐标。</summary>
    public override Task<ScreenPointsResponse> FindPixelsRgbMultiple(FindPixelsRgbMultipleRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindPixelsRgbMultiple(request)));

    /// <summary>在指定 ROI 内查找与目标 HSV 颜色匹配的像素坐标。</summary>
    public override Task<ScreenPointsResponse> FindPixelsHsv(FindPixelsHsvRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindPixelsHsv(request)));

    /// <summary>统计指定 ROI 内与目标 RGB 颜色匹配的像素数量。</summary>
    public override Task<CountPixelsRgbResponse> CountPixelsRgb(CountPixelsRgbRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.CountPixelsRgb(request)));

    /// <summary>获取屏幕上指定坐标处的像素 RGB 颜色值。</summary>
    public override Task<PixelRgbResponse> GetPixelRgb(GetPixelRgbRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.GetPixelRgb(request)));

    /// <summary>检查窗口实际尺寸与 ROI 截图的一致性（DPI 感知）。</summary>
    public override Task<WindowRoiConsistencyResponse> CheckWindowRoiConsistency(WindowRoiConsistencyRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.CheckWindowRoiConsistency(request)));

    /// <summary>截取全屏图像并按指定格式输出。</summary>
    public override Task<CvTextPayloadResponse> CaptureFullScreen(CaptureFullScreenRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.CaptureFullScreen(request)));

    /// <summary>截取屏幕指定区域的图像并按指定格式输出。</summary>
    public override Task<CvTextPayloadResponse> CaptureRegion(CaptureRegionRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.CaptureRegion(request)));
}
