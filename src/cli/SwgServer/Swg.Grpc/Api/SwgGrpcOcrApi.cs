using System.Globalization;
using Swg.CV;
using Swg.OCR;
using Swg.OCR.QuickTable;
using Swg.Grpc.Ocr;

namespace Swg.Grpc.Api;

/// <summary>
/// OCR（光学字符识别）gRPC 门面：封装 <c>Swg.OCR</c> 能力，与对应 Proto RPC 语义一致。
/// <para>
/// 提供屏幕/图像文字识别、表格识别、带匹配的文本识别等功能。
/// 支持 PaddleSharp 和 Tesseract 两种识别引擎，以及中文和英文两种语言。
/// </para>
/// <para>对应 Proto 服务：<c>swg.ocr.OcrService</c></para>
/// <para>
/// 异常映射约定（经由 <see cref="GrpcRouteRunner"/> 统一处理）：
/// <list type="bullet">
///   <item><description><see cref="ArgumentException"/> → <c>StatusCode.InvalidArgument</c></description></item>
///   <item><description><see cref="InvalidOperationException"/> → <c>StatusCode.Unavailable</c></description></item>
///   <item><description>其他异常 → <c>StatusCode.Internal</c></description></item>
/// </list>
/// </para>
/// </summary>
public static class SwgGrpcOcrApi
{
    /// <summary>
    /// 对屏幕指定区域进行 OCR 文字识别，返回所有识别到的文本行。
    /// </summary>
    /// <param name="request">
    /// 识别请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕识别区域（Left, Top, Width, Height）</description></item>
    ///   <item><description><c>Engine</c>（string，可选）：识别引擎，<c>PaddleSharp</c>（默认）或 <c>Tesseract</c></description></item>
    ///   <item><description><c>Language</c>（string，可选）：识别语言，<c>Chinese</c>（默认）或 <c>English</c></description></item>
    ///   <item><description><c>PaddleChineseModel</c>（string，可选）：PaddleSharp 中文模型版本，<c>V3</c>（默认）/ <c>V4</c> / <c>V5</c></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="OcrStringsResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Items</c>（repeated <see cref="OcrStringLineItem"/>）：识别到的文本行列表</description></item>
    /// </list>
    /// <para>每个 <see cref="OcrStringLineItem"/> 包含：</para>
    /// <list type="bullet">
    ///   <item><description><c>Text</c>（string）：识别到的文本内容</description></item>
    ///   <item><description><c>Left</c>/<c>Top</c>/<c>Width</c>/<c>Height</c>（int32）：文本行区域坐标</description></item>
    ///   <item><description><c>CenterX</c>/<c>CenterY</c>（double）：文本行中心点坐标</description></item>
    ///   <item><description><c>Confidence</c>（double）：识别置信度（0.0-1.0）</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Roi</c> 未提供/尺寸非法，或引擎/语言值无效</exception>
    public static OcrStringsResponse RecognizeScreenStrings(OcrScreenStringsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        OcrOptions opt = OcrGrpcOptionMapper.ToOptions(request.Engine, request.Language, request.PaddleChineseModel);
        IReadOnlyList<OcrStringLineResult> lines = SwgOcr.RecognizeScreenStrings(roi, opt);
        return ToStringsResponse(lines);
    }

    /// <summary>
    /// 对屏幕指定区域进行 OCR 文字识别，仅返回包含指定匹配文本的行。
    /// <para>服务端不对 <c>MatchText</c> 执行 Trim 操作，需调用方自行处理。</para>
    /// </summary>
    /// <param name="request">
    /// 识别请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕识别区域</description></item>
    ///   <item><description><c>MatchText</c>（string，必填）：要匹配的文本，不能为空字符串</description></item>
    ///   <item><description><c>Engine</c>/<c>Language</c>/<c>PaddleChineseModel</c>：同 <see cref="RecognizeScreenStrings"/></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="OcrStringsResponse"/>，仅包含匹配的文本行，字段同 <see cref="RecognizeScreenStrings"/>。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>MatchText</c> 为 null 或空字符串，或 <c>Roi</c> 非法</exception>
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

    /// <summary>
    /// 对屏幕指定区域进行表格结构识别，返回单元格信息。
    /// <para>
    /// 仅支持 <c>Engine=PaddleSharp</c>。使用 PaddleSharp 内置的表格识别模型。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 识别请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕识别区域</description></item>
    ///   <item><description><c>Engine</c>（string，可选）：必须为 <c>PaddleSharp</c></description></item>
    ///   <item><description><c>Language</c>/<c>PaddleChineseModel</c>：同 <see cref="RecognizeScreenStrings"/></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="OcrTableResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Cells</c>（repeated <see cref="OcrTableCellItem"/>）：表格单元格列表</description></item>
    /// </list>
    /// <para>每个 <see cref="OcrTableCellItem"/> 包含：</para>
    /// <list type="bullet">
    ///   <item><description><c>Text</c>（string）：单元格文本</description></item>
    ///   <item><description><c>Left</c>/<c>Top</c>/<c>Width</c>/<c>Height</c>（int32）：单元格区域坐标</description></item>
    ///   <item><description><c>Row</c>/<c>Column</c>（int32）：单元格行列索引</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Roi</c> 非法，或引擎非 PaddleSharp</exception>
    public static OcrTableResponse RecognizeScreenTable(OcrScreenTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        OcrOptions opt = OcrGrpcOptionMapper.ToOptions(request.Engine, request.Language, request.PaddleChineseModel, table: true);
        IReadOnlyList<OcrTableCellResult> cells = SwgOcr.RecognizeScreenTable(roi, opt);
        return ToTableResponse(cells);
    }

    /// <summary>
    /// 对图像文件进行 OCR 文字识别。
    /// <para>图像可通过文件路径、Base64 编码或 Data URL 提供。</para>
    /// </summary>
    /// <param name="request">
    /// 识别请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Image</c>（string，必填）：图像路径或 Base64 / Data URL 编码</description></item>
    ///   <item><description><c>Engine</c>/<c>Language</c>/<c>PaddleChineseModel</c>：同 <see cref="RecognizeScreenStrings"/></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="OcrStringsResponse"/>，字段同 <see cref="RecognizeScreenStrings"/>。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Image</c> 为空</exception>
    public static OcrStringsResponse RecognizeImageStrings(OcrImageStringsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Image))
            throw new ArgumentException("Image 必填（路径或 Base64 / Data URL）。");
        OcrOptions opt = OcrGrpcOptionMapper.ToOptions(request.Engine, request.Language, request.PaddleChineseModel);
        IReadOnlyList<OcrStringLineResult> lines = SwgOcr.RecognizeImageStrings(request.Image, opt);
        return ToStringsResponse(lines);
    }

    /// <summary>
    /// 对图像文件进行表格结构识别。
    /// <para>仅支持 <c>Engine=PaddleSharp</c>。</para>
    /// </summary>
    /// <param name="request">
    /// 识别请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Image</c>（string，必填）：图像路径或 Base64 / Data URL 编码</description></item>
    ///   <item><description><c>Engine</c>/<c>Language</c>/<c>PaddleChineseModel</c>：同 <see cref="RecognizeScreenTable"/></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="OcrTableResponse"/>，字段同 <see cref="RecognizeScreenTable"/>。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Image</c> 为空，或引擎非 PaddleSharp</exception>
    public static OcrTableResponse RecognizeImageTable(OcrImageTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Image))
            throw new ArgumentException("Image 必填（路径或 Base64 / Data URL）。");
        OcrOptions opt = OcrGrpcOptionMapper.ToOptions(request.Engine, request.Language, request.PaddleChineseModel, table: true);
        IReadOnlyList<OcrTableCellResult> cells = SwgOcr.RecognizeImageTable(request.Image, opt);
        return ToTableResponse(cells);
    }

    /// <summary>
    /// 对屏幕指定区域使用 QuickTable 算法进行快速表格检测与识别。
    /// <para>
    /// QuickTable 基于线条检测和交叉点分析实现表格结构识别，
    /// 适用于规则线条表格，速度较快。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 识别请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕识别区域</description></item>
    ///   <item><description><c>Language</c>（string，可选）：<c>Chinese</c>（默认）或 <c>English</c></description></item>
    ///   <item><description><c>Debug</c>（bool）：是否启用调试输出</description></item>
    ///   <item><description><c>SaveCellDebugImages</c>（bool）：是否保存单元格调试图像</description></item>
    ///   <item><description><c>DebugOutputDirectory</c>（string，可选）：调试图像输出目录</description></item>
    ///   <item><description><c>DebugImageBaseName</c>（string，可选）：调试图像文件名前缀（默认 <c>quicktable</c>）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="OcrTableResponse"/>，字段同 <see cref="RecognizeScreenTable"/>。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Roi</c> 非法</exception>
    public static OcrTableResponse RecognizeScreenQuickTable(OcrScreenQuickTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        QuickTableDetectOptions q = ToQuickTableOptions(request);
        IReadOnlyList<OcrTableCellResult> cells = SwgOcr.RecognizeScreenQuickTable(roi, q);
        return ToTableResponse(cells);
    }

    /// <summary>
    /// 对图像文件使用 QuickTable 算法进行快速表格检测与识别。
    /// </summary>
    /// <param name="request">
    /// 识别请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Image</c>（string，必填）：图像路径或 Base64 / Data URL 编码</description></item>
    ///   <item><description>其余参数同 <see cref="RecognizeScreenQuickTable"/></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="OcrTableResponse"/>，字段同 <see cref="RecognizeScreenTable"/>。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Image</c> 为空</exception>
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

/// <summary>
/// OCR 引擎选项映射器：将 gRPC 层的字符串参数解析为领域层枚举类型。
/// </summary>
internal static class OcrGrpcOptionMapper
{
    /// <summary>
    /// 将 gRPC 层参数组合为 <see cref="OcrOptions"/>，并执行参数验证。
    /// </summary>
    /// <param name="engine">引擎名称字符串（可选）</param>
    /// <param name="language">语言名称字符串（可选）</param>
    /// <param name="paddleChineseModel">PaddleSharp 中文模型版本（可选）</param>
    /// <param name="table">是否为表格识别模式（表格模式仅支持 PaddleSharp）</param>
    /// <returns>解析后的 <see cref="OcrOptions"/></returns>
    /// <exception cref="ArgumentException">参数值无效，或表格模式下引擎非 PaddleSharp</exception>
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
