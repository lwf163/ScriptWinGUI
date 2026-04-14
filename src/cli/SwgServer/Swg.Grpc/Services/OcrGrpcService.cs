using Grpc.Core;
using Swg.Grpc.Api;
using Swg.Grpc.Ocr;

namespace Swg.Grpc.Services;

public sealed class OcrGrpcService : OcrService.OcrServiceBase
{
    public override Task<OcrStringsResponse> RecognizeScreenStrings(OcrScreenStringsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeScreenStrings(request)));

    public override Task<OcrStringsResponse> RecognizeScreenStringsMatch(OcrScreenMatchRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeScreenStringsMatch(request)));

    public override Task<OcrTableResponse> RecognizeScreenTable(OcrScreenTableRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeScreenTable(request)));

    public override Task<OcrStringsResponse> RecognizeImageStrings(OcrImageStringsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeImageStrings(request)));

    public override Task<OcrTableResponse> RecognizeImageTable(OcrImageTableRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeImageTable(request)));

    public override Task<OcrTableResponse> RecognizeScreenQuickTable(OcrScreenQuickTableRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeScreenQuickTable(request)));

    public override Task<OcrTableResponse> RecognizeImageQuickTable(OcrImageQuickTableRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeImageQuickTable(request)));
}
