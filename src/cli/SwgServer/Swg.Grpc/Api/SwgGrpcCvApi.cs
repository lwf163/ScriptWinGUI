using System.Globalization;
using OpenCvSharp;
using Swg.CV;
using Swg.Grpc.Cv;

namespace Swg.Grpc.Api;

/// <summary>
/// CV gRPC 门面：封装 <c>Swg.CV</c> 能力，与对应 Proto RPC 语义一致。
/// </summary>
public static class SwgGrpcCvApi
{
    public static FindSingleTemplateResponse FindSingleTemplate(FindSingleTemplateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Template))
            throw new ArgumentException("Template 必填（路径或 Base64）。");
        ScreenRoi roi = RequireRoi(request.Roi);
        FindImageResult result = SwgCv.FindSingleTemplate(roi, request.Template, request.Threshold);
        return ToFindSingleTemplateResponse(result);
    }

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

    public static FindAllTemplatesResponse FindEachTemplateAtLeastOnce(FindAllTemplatesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        FindAllTemplatesResult result = SwgCv.FindEachTemplateAtLeastOnce(roi, request.Templates.ToList(), request.Threshold);
        return ToFindAllTemplatesResponse(result);
    }

    public static ScreenPointsResponse FindPixelsRgb(FindPixelsRgbRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        if (request.Rgb is null)
            throw new ArgumentException("Rgb 必填。");
        IReadOnlyList<global::Swg.CV.ScreenPoint> points = SwgCv.FindPixelsRgb(roi, ToRgbColor(request.Rgb));
        return ToScreenPointsResponse(points);
    }

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

    public static CountPixelsRgbResponse CountPixelsRgb(CountPixelsRgbRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ScreenRoi roi = RequireRoi(request.Roi);
        if (request.Rgb is null)
            throw new ArgumentException("Rgb 必填。");
        int count = SwgCv.CountPixelsRgb(roi, ToRgbColor(request.Rgb));
        return new CountPixelsRgbResponse { Count = count };
    }

    public static PixelRgbResponse GetPixelRgb(GetPixelRgbRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!SwgCv.TryGetScreenPixelRgb(request.X, request.Y, out RgbColor rgb))
            return new PixelRgbResponse { Success = false };
        return new PixelRgbResponse { Success = true, R = rgb.R, G = rgb.G, B = rgb.B };
    }

    public static WindowRoiConsistencyResponse CheckWindowRoiConsistency(WindowRoiConsistencyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        nint hwnd = ParseWindowHandle(request.WindowHandle);
        WindowRoiConsistencyResult result = SwgCv.CheckWindowRoiCaptureConsistency(hwnd);
        return ToWindowRoiConsistencyResponse(result);
    }

    public static CvTextPayloadResponse CaptureFullScreen(CaptureFullScreenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var options = CvGrpcScreenshotOptionsMapper.ToOptions(request.Options);
        string output = SwgCv.CaptureFullScreen(options);
        return new CvTextPayloadResponse { Output = output };
    }

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
/// gRPC <see cref="ScreenshotOptions"/> → <see cref="ScreenshotCaptureOptions"/>。
/// </summary>
internal static class CvGrpcScreenshotOptionsMapper
{
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
