using System.Globalization;
using Swg.CV;
using Swg.OCR;
using Swg.OCR.QuickTable;
using Swg.Grpc.Ocr;

namespace Swg.Grpc.Api;

/// <summary>
/// OCR gRPC 门面：封装 <c>Swg.OCR</c> 能力，与对应 Proto RPC 语义一致。
/// </summary>
public static class SwgGrpcOcrApi
{
    public static OcrStringsResponse RecognizeScreenStrings(OcrScreenStringsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        OcrOptions opt = OcrGrpcOptionMapper.ToOptions(request.Engine, request.Language, request.PaddleChineseModel);
        IReadOnlyList<OcrStringLineResult> lines = SwgOcr.RecognizeScreenStrings(roi, opt);
        return ToStringsResponse(lines);
    }

    public static OcrStringsResponse RecognizeScreenStringsMatch(OcrScreenMatchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.MatchText is null)
            throw new ArgumentException("MatchText 必填（服务端不 Trim）。");
        if (request.MatchText.Length == 0)
            throw new ArgumentException("MatchText 不能为空字符串。");
        ScreenRoi roi = RequireRoi(request.Roi);
        OcrOptions opt = OcrGrpcOptionMapper.ToOptions(request.Engine, request.Language, request.PaddleChineseModel);
        IReadOnlyList<OcrStringLineResult> lines = SwgOcr.RecognizeScreenStringsContaining(roi, request.MatchText, opt);
        return ToStringsResponse(lines);
    }

    public static OcrTableResponse RecognizeScreenTable(OcrScreenTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        OcrOptions opt = OcrGrpcOptionMapper.ToOptions(request.Engine, request.Language, request.PaddleChineseModel, table: true);
        IReadOnlyList<OcrTableCellResult> cells = SwgOcr.RecognizeScreenTable(roi, opt);
        return ToTableResponse(cells);
    }

    public static OcrStringsResponse RecognizeImageStrings(OcrImageStringsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Image))
            throw new ArgumentException("Image 必填（路径或 Base64 / Data URL）。");
        OcrOptions opt = OcrGrpcOptionMapper.ToOptions(request.Engine, request.Language, request.PaddleChineseModel);
        IReadOnlyList<OcrStringLineResult> lines = SwgOcr.RecognizeImageStrings(request.Image, opt);
        return ToStringsResponse(lines);
    }

    public static OcrTableResponse RecognizeImageTable(OcrImageTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Image))
            throw new ArgumentException("Image 必填（路径或 Base64 / Data URL）。");
        OcrOptions opt = OcrGrpcOptionMapper.ToOptions(request.Engine, request.Language, request.PaddleChineseModel, table: true);
        IReadOnlyList<OcrTableCellResult> cells = SwgOcr.RecognizeImageTable(request.Image, opt);
        return ToTableResponse(cells);
    }

    public static OcrTableResponse RecognizeScreenQuickTable(OcrScreenQuickTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        QuickTableDetectOptions q = ToQuickTableOptions(request);
        IReadOnlyList<OcrTableCellResult> cells = SwgOcr.RecognizeScreenQuickTable(roi, q);
        return ToTableResponse(cells);
    }

    public static OcrTableResponse RecognizeImageQuickTable(OcrImageQuickTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Image))
            throw new ArgumentException("Image 必填（路径或 Base64 / Data URL）。");
        QuickTableDetectOptions q = ToQuickTableOptions(request);
        IReadOnlyList<OcrTableCellResult> cells = SwgOcr.RecognizeImageQuickTable(request.Image, q);
        return ToTableResponse(cells);
    }

    private static QuickTableDetectOptions ToQuickTableOptions(OcrScreenQuickTableRequest body) =>
        new()
        {
            Language = OcrGrpcOptionMapper.ParseLanguage(body.Language),
            Debug = body.Debug,
            SaveCellDebugImages = body.SaveCellDebugImages,
            DebugOutputDirectory = string.IsNullOrWhiteSpace(body.DebugOutputDirectory) ? null : body.DebugOutputDirectory.Trim(),
            DebugImageBaseName = string.IsNullOrWhiteSpace(body.DebugImageBaseName) ? "quicktable" : body.DebugImageBaseName.Trim(),
        };

    private static QuickTableDetectOptions ToQuickTableOptions(OcrImageQuickTableRequest body) =>
        new()
        {
            Language = OcrGrpcOptionMapper.ParseLanguage(body.Language),
            Debug = body.Debug,
            SaveCellDebugImages = body.SaveCellDebugImages,
            DebugOutputDirectory = string.IsNullOrWhiteSpace(body.DebugOutputDirectory) ? null : body.DebugOutputDirectory.Trim(),
            DebugImageBaseName = string.IsNullOrWhiteSpace(body.DebugImageBaseName) ? "quicktable" : body.DebugImageBaseName.Trim(),
        };

    private static ScreenRoi RequireRoi(Roi? roi)
    {
        if (roi is null)
            throw new ArgumentException("Roi 必填（Left, Top, Width, Height）。");
        if (roi.Width <= 0 || roi.Height <= 0)
            throw new ArgumentException("Roi: Width/Height 必须为正。");
        return new ScreenRoi(roi.Left, roi.Top, roi.Width, roi.Height);
    }

    private static OcrStringsResponse ToStringsResponse(IReadOnlyList<OcrStringLineResult> lines)
    {
        var r = new OcrStringsResponse();
        r.Items.AddRange(lines.Select(static x => new OcrStringLineItem
        {
            Text = x.Text,
            Left = x.Left,
            Top = x.Top,
            Width = x.Width,
            Height = x.Height,
            CenterX = x.CenterX,
            CenterY = x.CenterY,
            Confidence = x.Confidence,
        }));
        return r;
    }

    private static OcrTableResponse ToTableResponse(IReadOnlyList<OcrTableCellResult> cells)
    {
        var r = new OcrTableResponse();
        r.Cells.AddRange(cells.Select(static x => new OcrTableCellItem
        {
            Text = x.Text,
            Left = x.Left,
            Top = x.Top,
            Width = x.Width,
            Height = x.Height,
            Row = x.Row,
            Column = x.Column,
        }));
        return r;
    }
}

internal static class OcrGrpcOptionMapper
{
    public static OcrOptions ToOptions(string? engine, string? language, string? paddleChineseModel, bool table = false)
    {
        OcrEngineKind e = ParseEngine(engine);
        OcrLanguage lang = ParseLanguage(language);
        PaddleChineseModelVersion zh = ParsePaddleChinese(paddleChineseModel);

        if (table && e != OcrEngineKind.PaddleSharp)
            throw new ArgumentException("表格识别仅支持 Engine=PaddleSharp。");

        return new OcrOptions(e, lang, zh);
    }

    private static OcrEngineKind ParseEngine(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return OcrEngineKind.PaddleSharp;
        if (Enum.TryParse<OcrEngineKind>(raw.Trim(), true, out var v))
            return v;
        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "无效的 Engine: {0}，应为 PaddleSharp 或 Tesseract。", raw));
    }

    internal static OcrLanguage ParseLanguage(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return OcrLanguage.Chinese;
        if (Enum.TryParse<OcrLanguage>(raw.Trim(), true, out var v))
            return v;
        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "无效的 Language: {0}，应为 Chinese 或 English。", raw));
    }

    private static PaddleChineseModelVersion ParsePaddleChinese(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return PaddleChineseModelVersion.V3;
        string s = raw.Trim();
        if (s.Equals("ChineseV3", StringComparison.OrdinalIgnoreCase) || s.Equals("V3", StringComparison.OrdinalIgnoreCase))
            return PaddleChineseModelVersion.V3;
        if (s.Equals("ChineseV4", StringComparison.OrdinalIgnoreCase) || s.Equals("V4", StringComparison.OrdinalIgnoreCase))
            return PaddleChineseModelVersion.V4;
        if (s.Equals("ChineseV5", StringComparison.OrdinalIgnoreCase) || s.Equals("V5", StringComparison.OrdinalIgnoreCase))
            return PaddleChineseModelVersion.V5;
        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "无效的 PaddleChineseModel: {0}，应为 ChineseV3 / ChineseV4 / ChineseV5（或 V3/V4/V5）。", raw));
    }
}
