using OpenCvSharp;

namespace Swg.CV;

/// <summary>
/// 屏幕 ROI：左上角 (left, top) + 宽度 + 高度（逻辑像素）。
/// </summary>
public readonly record struct ScreenRoi(int Left, int Top, int Width, int Height)
{
    /// <summary>像素面积 width × height。</summary>
    public int Area => Width * Height;

    /// <summary>校验尺寸是否为正。</summary>
    public bool IsValid => Width > 0 && Height > 0;
}

/// <summary>多模板找其一：首个达标或全局最高分。</summary>
public enum TemplateMatchPreference
{
    /// <summary>按列表顺序，第一个分数 ≥ 阈值即返回。</summary>
    FirstQualified,

    /// <summary>遍历全部模板，返回分数最高且 ≥ 阈值的一条（若无则未找到）。</summary>
    BestScore,
}

/// <summary>截图结果交付形式。</summary>
public enum ScreenshotOutputKind
{
    /// <summary>写入磁盘，返回路径字符串。</summary>
    FilePath,

    /// <summary>返回 Base64 字符串。</summary>
    Base64,
}

/// <summary>截图编码格式（对应 Cv2.ImEncode）。</summary>
public enum ScreenshotImageFormat
{
    Png,
    Jpeg,
}

/// <summary>Base64 是否带 Data URL 前缀。</summary>
public enum ScreenshotBase64Variant
{
    /// <summary>仅 Base64 正文。</summary>
    Raw,

    /// <summary>如 data:image/png;base64,</summary>
    DataUrl,
}

/// <summary>§7、§8 截图参数。</summary>
public sealed class ScreenshotCaptureOptions
{
    /// <summary>输出形式（必选）。</summary>
    public ScreenshotOutputKind OutputKind { get; init; }

    /// <summary>当 <see cref="OutputKind"/> 为 <see cref="ScreenshotOutputKind.FilePath"/> 时必填。</summary>
    public string? TargetFilePath { get; init; }

    /// <summary>图像编码；Base64 时必选，写文件时建议与扩展名一致。</summary>
    public ScreenshotImageFormat ImageFormat { get; init; } = ScreenshotImageFormat.Png;

    /// <summary>Base64 时的前缀形态。</summary>
    public ScreenshotBase64Variant Base64Variant { get; init; } = ScreenshotBase64Variant.Raw;

    /// <summary>JPEG 质量 0–100，默认 90。</summary>
    public int JpegQuality { get; init; } = 90;
}

/// <summary>单模板找图结果（矩形为屏幕绝对坐标）。</summary>
public readonly record struct FindImageResult(bool Found, double Score, int Left, int Top, int Width, int Height);

/// <summary>多模板找其一结果。</summary>
public readonly record struct FindOneOfTemplatesResult(
    bool Found,
    int TemplateIndex,
    double Score,
    int Left,
    int Top,
    int Width,
    int Height);

/// <summary>每个模板的匹配结果（屏幕绝对坐标）。</summary>
public readonly record struct TemplateMatchItem(int TemplateIndex, bool Found, double Score, int Left, int Top, int Width, int Height);

/// <summary>多模板「每模板至少一次」结果。</summary>
public readonly record struct FindAllTemplatesResult(bool AllFound, IReadOnlyList<TemplateMatchItem> Items);

/// <summary>屏幕上的像素坐标（绝对）。</summary>
public readonly record struct ScreenPoint(int X, int Y);

/// <summary>
/// 对外使用的 RGB 颜色（红、绿、蓝）；与 CSS / 常见 UI 约定一致。
/// 库内在与 OpenCV <c>Mat</c>（BGR）交互时会转换为 BGR。
/// </summary>
public readonly record struct RgbColor(byte R, byte G, byte B);

/// <summary>
/// 窗口矩形与截屏像素一致性检查结果。
/// </summary>
public readonly record struct WindowRoiConsistencyResult(
    bool Success,
    ScreenRoi Roi,
    int CapturedWidth,
    int CapturedHeight,
    uint WindowDpi,
    bool IsPixelSizeMatched);
