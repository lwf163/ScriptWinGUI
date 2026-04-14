using Grpc.Core;
using Swg.Grpc.Api;
using Swg.Grpc.Cv;

namespace Swg.Grpc.Services;

public sealed class CvGrpcService : CvService.CvServiceBase
{
    public override Task<FindSingleTemplateResponse> FindSingleTemplate(FindSingleTemplateRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindSingleTemplate(request)));

    public override Task<FindOneOfTemplatesResponse> FindOneOfTemplates(FindOneOfTemplatesRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindOneOfTemplates(request)));

    public override Task<FindAllTemplatesResponse> FindEachTemplateAtLeastOnce(FindAllTemplatesRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindEachTemplateAtLeastOnce(request)));

    public override Task<ScreenPointsResponse> FindPixelsRgb(FindPixelsRgbRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindPixelsRgb(request)));

    public override Task<ScreenPointsResponse> FindPixelsRgbMultiple(FindPixelsRgbMultipleRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindPixelsRgbMultiple(request)));

    public override Task<ScreenPointsResponse> FindPixelsHsv(FindPixelsHsvRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.FindPixelsHsv(request)));

    public override Task<CountPixelsRgbResponse> CountPixelsRgb(CountPixelsRgbRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.CountPixelsRgb(request)));

    public override Task<PixelRgbResponse> GetPixelRgb(GetPixelRgbRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.GetPixelRgb(request)));

    public override Task<WindowRoiConsistencyResponse> CheckWindowRoiConsistency(WindowRoiConsistencyRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.CheckWindowRoiConsistency(request)));

    public override Task<CvTextPayloadResponse> CaptureFullScreen(CaptureFullScreenRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.CaptureFullScreen(request)));

    public override Task<CvTextPayloadResponse> CaptureRegion(CaptureRegionRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCvApi.CaptureRegion(request)));
}
