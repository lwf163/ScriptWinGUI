using System.Globalization;
using OpenCvSharp;
using Swg.CV;
using Swg.Grpc.Cv;

namespace Swg.Grpc.Api;

/// <summary>
/// CV（计算机视觉）gRPC 门面：封装 <c>Swg.CV</c> 能力，与对应 Proto RPC 语义一致。
/// <para>
/// 提供屏幕模板匹配、像素查找/计数、屏幕截图捕获、窗口 ROI 一致性检测等功能。
/// 所有方法均为无状态静态方法，线程安全。
/// </para>
/// <para>对应 Proto 服务：<c>swg.cv.CvService</c></para>
/// <para>
/// 异常映射约定（经由 <see cref="GrpcRouteRunner"/> 统一处理）：
/// <list type="bullet">
///   <item><description><see cref="ArgumentException"/> → <c>StatusCode.InvalidArgument</c></description></item>
///   <item><description><see cref="InvalidOperationException"/> → <c>StatusCode.Unavailable</c></description></item>
///   <item><description>其他异常 → <c>StatusCode.Internal</c></description></item>
/// </list>
/// </para>
/// </summary>
public static class SwgGrpcCvApi
{
    /// <summary>
    /// 在指定屏幕区域内查找单个模板图像的最佳匹配位置。
    /// <para>
    /// 使用 OpenCV 模板匹配算法，在 ROI 对应的屏幕截图中搜索与模板最相似的区域。
    /// 模板可通过文件路径或 Base64 编码字符串提供。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕搜索区域（Left, Top, Width, Height，Width/Height 必须为正）</description></item>
    ///   <item><description><c>Template</c>（string，必填）：模板图像，支持文件路径或 Base64 编码</description></item>
    ///   <item><description><c>Threshold</c>（double）：匹配阈值（0.0-1.0），低于此值视为未找到</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="FindSingleTemplateResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Found</c>（bool）：是否找到匹配</description></item>
    ///   <item><description><c>Score</c>（double）：匹配得分（0.0-1.0，越高越相似）</description></item>
    ///   <item><description><c>Left</c>/<c>Top</c>/<c>Width</c>/<c>Height</c>（int32）：匹配区域在屏幕上的绝对坐标</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Template</c> 为空或 <c>Roi</c> 未提供/尺寸非法</exception>
    public static FindSingleTemplateResponse FindSingleTemplate(FindSingleTemplateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Template))
            throw new ArgumentException("Template 必填（路径或 Base64）。");
        ScreenRoi roi = RequireRoi(request.Roi);
        FindImageResult result = SwgCv.FindSingleTemplate(roi, request.Template, request.Threshold);
        return ToFindSingleTemplateResponse(result);
    }

    /// <summary>
    /// 在指定屏幕区域内从一组候选模板中查找最先（或最优）匹配的模板。
    /// <para>
    /// 依次对每个模板执行匹配，根据 <c>Preference</c> 策略返回结果：
    /// <c>FirstQualified</c>（默认）返回首个超过阈值的匹配，<c>BestScore</c> 返回得分最高的匹配。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕搜索区域</description></item>
    ///   <item><description><c>Templates</c>（repeated string，必填）：候选模板图像列表，不可为空</description></item>
    ///   <item><description><c>Threshold</c>（double）：匹配阈值</description></item>
    ///   <item><description><c>Preference</c>（string，可选）：匹配策略，<c>FirstQualified</c>（默认）或 <c>BestScore</c></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="FindOneOfTemplatesResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Found</c>（bool）：是否找到匹配</description></item>
    ///   <item><description><c>TemplateIndex</c>（int32）：匹配到的模板在列表中的索引</description></item>
    ///   <item><description><c>Score</c>（double）：匹配得分</description></item>
    ///   <item><description><c>Left</c>/<c>Top</c>/<c>Width</c>/<c>Height</c>（int32）：匹配区域坐标</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Templates</c> 为空、<c>Preference</c> 值非法或 <c>Roi</c> 非法</exception>
    public static FindOneOfTemplatesResponse FindOneOfTemplates(FindOneOfTemplatesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Templates.Count == 0)
            throw new ArgumentException("Templates 不能为空。");
        ScreenRoi roi = RequireRoi(request.Roi);
        var pref = ParsePreference(request.Preference);
        FindOneOfTemplatesResult result = SwgCv.FindOneOfTemplates(roi, request.Templates.ToList(), request.Threshold, pref);
        return ToFindOneOfTemplatesResponse(result);
    }

    /// <summary>
    /// 在指定屏幕区域内查找多个模板，确保每个模板至少匹配到一次。
    /// <para>
    /// 对模板列表中的每个模板分别执行匹配，返回每个模板的匹配结果。
    /// 即使某些模板未匹配到，也会返回对应条目（<c>Found = false</c>）。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕搜索区域</description></item>
    ///   <item><description><c>Templates</c>（repeated string）：模板图像列表，为空时 <c>AllFound</c> 为 true</description></item>
    ///   <item><description><c>Threshold</c>（double）：匹配阈值</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="FindAllTemplatesResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>AllFound</c>（bool）：是否所有模板均匹配成功</description></item>
    ///   <item><description><c>Items</c>（repeated <see cref="TemplateMatchItem"/>）：每个模板的匹配结果，含 <c>TemplateIndex</c>/<c>Found</c>/<c>Score</c>/<c>Left</c>/<c>Top</c>/<c>Width</c>/<c>Height</c></description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FindAllTemplatesResponse FindEachTemplateAtLeastOnce(FindAllTemplatesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        FindAllTemplatesResult result = SwgCv.FindEachTemplateAtLeastOnce(roi, request.Templates.ToList(), request.Threshold);
        return ToFindAllTemplatesResponse(result);
    }

    /// <summary>
    /// 在指定屏幕区域内查找与目标 RGB 颜色精确匹配的像素坐标。
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕搜索区域</description></item>
    ///   <item><description><c>Rgb</c>（<see cref="Rgb"/>，必填）：目标 RGB 颜色（R/G/B 各 0-255）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ScreenPointsResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Points</c>（repeated <see cref="ScreenPoint"/>）：匹配像素的绝对屏幕坐标列表，每个含 <c>X</c>/<c>Y</c></description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Rgb</c> 或 <c>Roi</c> 未提供</exception>
    public static ScreenPointsResponse FindPixelsRgb(FindPixelsRgbRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        if (request.Rgb is null)
            throw new ArgumentException("Rgb 必填。");
        IReadOnlyList<global::Swg.CV.ScreenPoint> points = SwgCv.FindPixelsRgb(roi, ToRgbColor(request.Rgb));
        return ToScreenPointsResponse(points);
    }

    /// <summary>
    /// 在指定屏幕区域内查找与任意一组 RGB 颜色匹配的像素坐标（多色查找）。
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕搜索区域</description></item>
    ///   <item><description><c>RgbColors</c>（repeated <see cref="Rgb"/>，必填）：目标 RGB 颜色列表，不可为空</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ScreenPointsResponse"/>，字段同 <see cref="FindPixelsRgb"/>。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>RgbColors</c> 为空或 <c>Roi</c> 非法</exception>
    public static ScreenPointsResponse FindPixelsRgbMultiple(FindPixelsRgbMultipleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        if (request.RgbColors.Count == 0)
            throw new ArgumentException("RgbColors 不能为空。");
        var list = request.RgbColors.Select(ToRgbColor).ToList();
        IReadOnlyList<global::Swg.CV.ScreenPoint> points = SwgCv.FindPixelsRgbMultiple(roi, list);
        return ToScreenPointsResponse(points);
    }

    /// <summary>
    /// 在指定屏幕区域内查找与目标 HSV 颜色匹配的像素坐标。
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕搜索区域</description></item>
    ///   <item><description><c>Hsv</c>（<see cref="Hsv"/>，必填）：目标 HSV 颜色（H/S/V 各为 double）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ScreenPointsResponse"/>，字段同 <see cref="FindPixelsRgb"/>。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Hsv</c> 或 <c>Roi</c> 未提供</exception>
    public static ScreenPointsResponse FindPixelsHsv(FindPixelsHsvRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        if (request.Hsv is null)
            throw new ArgumentException("Hsv 必填。");
        var s = new Scalar(request.Hsv.H, request.Hsv.S, request.Hsv.V);
        IReadOnlyList<global::Swg.CV.ScreenPoint> points = SwgCv.FindPixelsHsv(roi, s);
        return ToScreenPointsResponse(points);
    }

    /// <summary>
    /// 统计指定屏幕区域内与目标 RGB 颜色匹配的像素数量。
    /// </summary>
    /// <param name="request">
    /// 统计请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：屏幕搜索区域</description></item>
    ///   <item><description><c>Rgb</c>（<see cref="Rgb"/>，必填）：目标 RGB 颜色</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="CountPixelsRgbResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Count</c>（int32）：匹配像素数量</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Rgb</c> 或 <c>Roi</c> 未提供</exception>
    public static CountPixelsRgbResponse CountPixelsRgb(CountPixelsRgbRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        if (request.Rgb is null)
            throw new ArgumentException("Rgb 必填。");
        int count = SwgCv.CountPixelsRgb(roi, ToRgbColor(request.Rgb));
        return new CountPixelsRgbResponse { Count = count };
    }

    /// <summary>
    /// 获取屏幕上指定坐标处的像素 RGB 颜色值。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>X</c>（int32）：像素 X 坐标（屏幕绝对坐标）</description></item>
    ///   <item><description><c>Y</c>（int32）：像素 Y 坐标（屏幕绝对坐标）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="PixelRgbResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Success</c>（bool）：是否成功获取（坐标越界时为 false）</description></item>
    ///   <item><description><c>R</c>/<c>G</c>/<c>B</c>（int32）：像素颜色分量（仅 Success 为 true 时有效）</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static PixelRgbResponse GetPixelRgb(GetPixelRgbRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!SwgCv.TryGetScreenPixelRgb(request.X, request.Y, out RgbColor rgb))
            return new PixelRgbResponse { Success = false };
        return new PixelRgbResponse { Success = true, R = rgb.R, G = rgb.G, B = rgb.B };
    }

    /// <summary>
    /// 检查指定窗口的实际尺寸与其 ROI 截图的一致性（DPI 感知）。
    /// <para>
    /// 用于诊断高 DPI 环境下窗口截图尺寸与逻辑尺寸不匹配的问题。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>WindowHandle</c>（string，必填）：窗口句柄（HWND），支持十进制或 0x 十六进制格式</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="WindowRoiConsistencyResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Success</c>（bool）：检测是否成功执行</description></item>
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>）：窗口的 ROI 信息</description></item>
    ///   <item><description><c>CapturedWidth</c>/<c>CapturedHeight</c>（int32）：实际截取的像素尺寸</description></item>
    ///   <item><description><c>WindowDpi</c>（uint32）：窗口 DPI 值</description></item>
    ///   <item><description><c>IsPixelSizeMatched</c>（bool）：像素尺寸是否匹配</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>WindowHandle</c> 为空或格式无效</exception>
    public static WindowRoiConsistencyResponse CheckWindowRoiConsistency(WindowRoiConsistencyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        nint hwnd = ParseWindowHandle(request.WindowHandle);
        WindowRoiConsistencyResult result = SwgCv.CheckWindowRoiCaptureConsistency(hwnd);
        return ToWindowRoiConsistencyResponse(result);
    }

    /// <summary>
    /// 截取全屏图像并按指定格式输出。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Options</c>（<see cref="ScreenshotOptions"/>，可选）：截图选项，为 null 时使用默认值</description></item>
    /// </list>
    /// <para><see cref="ScreenshotOptions"/> 字段说明：</para>
    /// <list type="bullet">
    ///   <item><description><c>OutputKind</c>（string，可选）：<c>Base64</c>（默认）或 <c>FilePath</c></description></item>
    ///   <item><description><c>TargetFilePath</c>（string，条件必填）：OutputKind 为 FilePath 时必填</description></item>
    ///   <item><description><c>ImageFormat</c>（string，可选）：<c>Png</c>（默认）、<c>Jpeg</c> 或 <c>Bmp</c></description></item>
    ///   <item><description><c>Base64Variant</c>（string，可选）：<c>Raw</c>（默认）或 <c>DataUrl</c></description></item>
    ///   <item><description><c>JpegQuality</c>（int32，可选）：JPEG 质量（0-100，默认 0 即使用引擎默认值）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="CvTextPayloadResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Output</c>（string）：Base64 编码字符串或保存的文件路径</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException">OutputKind 为 FilePath 但 TargetFilePath 为空，或枚举值无效</exception>
    public static CvTextPayloadResponse CaptureFullScreen(CaptureFullScreenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var options = CvGrpcScreenshotOptionsMapper.ToOptions(request.Options);
        string output = SwgCv.CaptureFullScreen(options);
        return new CvTextPayloadResponse { Output = output };
    }

    /// <summary>
    /// 截取屏幕指定区域的图像并按指定格式输出。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Roi</c>（<see cref="Roi"/>，必填）：截取区域</description></item>
    ///   <item><description><c>Options</c>（<see cref="ScreenshotOptions"/>，可选）：截图选项，详见 <see cref="CaptureFullScreen"/></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="CvTextPayloadResponse"/>，字段同 <see cref="CaptureFullScreen"/>。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Roi</c> 未提供/尺寸非法，或截图选项参数无效</exception>
    public static CvTextPayloadResponse CaptureRegion(CaptureRegionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        var options = CvGrpcScreenshotOptionsMapper.ToOptions(request.Options);
        string output = SwgCv.CaptureRegion(roi, options);
        return new CvTextPayloadResponse { Output = output };
    }

    private static ScreenRoi RequireRoi(Roi? roi)
    {
        if (roi is null)
            throw new ArgumentException("Roi 必填（Left, Top, Width, Height）。");
        if (roi.Width <= 0 || roi.Height <= 0)
            throw new ArgumentException("Roi: Width/Height 必须为正。");
        return new ScreenRoi(roi.Left, roi.Top, roi.Width, roi.Height);
    }

    private static RgbColor ToRgbColor(Rgb rgb) =>
        new(
            (byte)Math.Clamp(rgb.R, 0, 255),
            (byte)Math.Clamp(rgb.G, 0, 255),
            (byte)Math.Clamp(rgb.B, 0, 255));

    private static nint ParseWindowHandle(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("WindowHandle 必填（HWND：十进制或 0x 十六进制）。");
        s = s.Trim();
        try
        {
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return (nint)Convert.ToUInt64(s, 16);
            return (nint)ulong.Parse(s, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("WindowHandle 格式无效。", ex);
        }
    }

    private static TemplateMatchPreference ParsePreference(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return TemplateMatchPreference.FirstQualified;
        if (Enum.TryParse<TemplateMatchPreference>(s, true, out var p))
            return p;
        throw new ArgumentException($"无效的 Preference: {s}，应为 FirstQualified 或 BestScore。");
    }

    private static FindSingleTemplateResponse ToFindSingleTemplateResponse(FindImageResult x) =>
        new()
        {
            Found = x.Found,
            Score = x.Score,
            Left = x.Left,
            Top = x.Top,
            Width = x.Width,
            Height = x.Height,
        };

    private static FindOneOfTemplatesResponse ToFindOneOfTemplatesResponse(FindOneOfTemplatesResult x) =>
        new()
        {
            Found = x.Found,
            TemplateIndex = x.TemplateIndex,
            Score = x.Score,
            Left = x.Left,
            Top = x.Top,
            Width = x.Width,
            Height = x.Height,
        };

    private static FindAllTemplatesResponse ToFindAllTemplatesResponse(FindAllTemplatesResult x) =>
        new()
        {
            AllFound = x.AllFound,
            Items =
            {
                x.Items.Select(static i => new Swg.Grpc.Cv.TemplateMatchItem
                {
                    TemplateIndex = i.TemplateIndex,
                    Found = i.Found,
                    Score = i.Score,
                    Left = i.Left,
                    Top = i.Top,
                    Width = i.Width,
                    Height = i.Height,
                }),
            },
        };

    private static ScreenPointsResponse ToScreenPointsResponse(IReadOnlyList<Swg.CV.ScreenPoint> points)
    {
        var r = new ScreenPointsResponse();
        r.Points.AddRange(points.Select(static p => new Swg.Grpc.Cv.ScreenPoint { X = p.X, Y = p.Y }));
        return r;
    }

    private static WindowRoiConsistencyResponse ToWindowRoiConsistencyResponse(WindowRoiConsistencyResult x) =>
        new()
        {
            Success = x.Success,
            Roi = new Roi
            {
                Left = x.Roi.Left,
                Top = x.Roi.Top,
                Width = x.Roi.Width,
                Height = x.Roi.Height,
            },
            CapturedWidth = x.CapturedWidth,
            CapturedHeight = x.CapturedHeight,
            WindowDpi = x.WindowDpi,
            IsPixelSizeMatched = x.IsPixelSizeMatched,
        };
}

/// <summary>
/// gRPC <see cref="ScreenshotOptions"/> → <see cref="ScreenshotCaptureOptions"/> 映射器。
/// </summary>
internal static class CvGrpcScreenshotOptionsMapper
{
    /// <summary>
    /// 将 gRPC 层的截图选项转换为领域层选项，并执行参数验证。
    /// </summary>
    /// <param name="dto">gRPC 截图选项，为 null 时使用默认值</param>
    /// <returns>转换后的 <see cref="ScreenshotCaptureOptions"/></returns>
    /// <exception cref="ArgumentException">OutputKind 为 FilePath 但 TargetFilePath 为空，或枚举值非法</exception>
    public static ScreenshotCaptureOptions ToOptions(ScreenshotOptions? dto)
    {
        dto ??= new ScreenshotOptions();
        var kind = ParseEnum(dto.OutputKind, ScreenshotOutputKind.Base64, static s =>
            Enum.TryParse<ScreenshotOutputKind>(s, true, out var v) ? v : null,
            nameof(ScreenshotOptions.OutputKind));
        var format = ParseEnum(dto.ImageFormat, ScreenshotImageFormat.Png, static s =>
            Enum.TryParse<ScreenshotImageFormat>(s, true, out var v) ? v : null,
            nameof(ScreenshotOptions.ImageFormat));
        var b64 = ParseEnum(dto.Base64Variant, ScreenshotBase64Variant.Raw, static s =>
            Enum.TryParse<ScreenshotBase64Variant>(s, true, out var v) ? v : null,
            nameof(ScreenshotOptions.Base64Variant));

        if (kind == ScreenshotOutputKind.FilePath && string.IsNullOrWhiteSpace(dto.TargetFilePath))
            throw new ArgumentException("OutputKind 为 FilePath 时 TargetFilePath 必填。");

        return new ScreenshotCaptureOptions
        {
            OutputKind = kind,
            TargetFilePath = dto.TargetFilePath,
            ImageFormat = format,
            Base64Variant = b64,
            JpegQuality = Math.Clamp(dto.JpegQuality, 0, 100),
        };
    }

    private static T ParseEnum<T>(string? raw, T defaultValue, Func<string, T?> tryParse, string fieldName)
        where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(raw))
            return defaultValue;
        T? v = tryParse(raw.Trim());
        if (v is null)
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "无效的 {0}: {1}", fieldName, raw));
        return v.Value;
    }
}
