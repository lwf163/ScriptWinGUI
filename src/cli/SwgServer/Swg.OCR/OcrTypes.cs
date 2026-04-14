namespace Swg.OCR;

/// <summary>OCR 引擎种类。</summary>
public enum OcrEngineKind
{
    /// <summary>PaddleSharp（默认）。</summary>
    PaddleSharp,

    /// <summary>Tesseract。</summary>
    Tesseract,
}

/// <summary>识别语言（仅中文 / 英文）。</summary>
public enum OcrLanguage
{
    /// <summary>中文（默认）。</summary>
    Chinese,

    /// <summary>英文。</summary>
    English,
}

/// <summary>Paddle 中文整图模型版本（与 <c>LocalFullModels</c> 对应）。</summary>
public enum PaddleChineseModelVersion
{
    /// <summary>ChineseV3。</summary>
    V3,

    /// <summary>ChineseV4。</summary>
    V4,

    /// <summary>ChineseV5。</summary>
    V5,
}

/// <summary>单次识别选项（引擎、语言、Paddle 中文模型）。</summary>
public readonly record struct OcrOptions(
    OcrEngineKind Engine = OcrEngineKind.PaddleSharp,
    OcrLanguage Language = OcrLanguage.Chinese,
    PaddleChineseModelVersion PaddleChineseModel = PaddleChineseModelVersion.V3);

/// <summary>单行 / 单块文字识别结果（相对当前图像或屏幕 ROI 左上角）。</summary>
public sealed record OcrStringLineResult(
    string Text,
    int Left,
    int Top,
    int Width,
    int Height,
    double CenterX,
    double CenterY,
    double Confidence);

/// <summary>表格单元识别结果（行列从 0 起，相对当前图像或 ROI）。</summary>
public sealed record OcrTableCellResult(
    string Text,
    int Left,
    int Top,
    int Width,
    int Height,
    int Row,
    int Column);
