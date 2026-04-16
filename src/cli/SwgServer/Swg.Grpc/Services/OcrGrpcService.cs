using Grpc.Core;
using Swg.Grpc.Api;
using Swg.Grpc.Ocr;

namespace Swg.Grpc.Services;

/// <summary>
/// OCR（光学字符识别）gRPC 服务实现：提供屏幕/图像文字识别、表格识别、QuickTable 快速表格检测。
/// <para>
/// 继承自 <c>OcrService.OcrServiceBase</c>，由 gRPC 运行时自动注册。
/// 所有 RPC 均通过 <see cref="GrpcRouteRunner"/> 统一异常映射。
/// </para>
/// <para>对应 Proto 定义：<c>swg.ocr.OcrService</c></para>
/// </summary>
public sealed class OcrGrpcService : OcrService.OcrServiceBase
{
    /// <summary>对屏幕指定区域进行 OCR 文字识别。</summary>
    public override Task<OcrStringsResponse> RecognizeScreenStrings(OcrScreenStringsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeScreenStrings(request)));

    /// <summary>对屏幕指定区域进行 OCR 识别，仅返回包含匹配文本的行。</summary>
    public override Task<OcrStringsResponse> RecognizeScreenStringsMatch(OcrScreenMatchRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeScreenStringsMatch(request)));

    /// <summary>对屏幕指定区域进行表格结构识别（仅 PaddleSharp）。</summary>
    public override Task<OcrTableResponse> RecognizeScreenTable(OcrScreenTableRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeScreenTable(request)));

    /// <summary>对图像文件进行 OCR 文字识别。</summary>
    public override Task<OcrStringsResponse> RecognizeImageStrings(OcrImageStringsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeImageStrings(request)));

    /// <summary>对图像文件进行表格结构识别（仅 PaddleSharp）。</summary>
    public override Task<OcrTableResponse> RecognizeImageTable(OcrImageTableRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeImageTable(request)));

    /// <summary>对屏幕指定区域使用 QuickTable 算法进行快速表格检测。</summary>
    public override Task<OcrTableResponse> RecognizeScreenQuickTable(OcrScreenQuickTableRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeScreenQuickTable(request)));

    /// <summary>对图像文件使用 QuickTable 算法进行快速表格检测。</summary>
    public override Task<OcrTableResponse> RecognizeImageQuickTable(OcrImageQuickTableRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcOcrApi.RecognizeImageQuickTable(request)));
}
