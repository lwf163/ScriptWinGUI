using Swg.OCR;

namespace Swg.OCR.QuickTable;

/// <summary>快速表格检测选项（基于线检测 + Tesseract）。</summary>
public sealed class QuickTableDetectOptions
{
    /// <summary>Tesseract 语言（中文 chi_sim / 英文 eng）。</summary>
    public OcrLanguage Language { get; init; } = OcrLanguage.Chinese;

    /// <summary>是否输出调试日志（<see cref="System.Diagnostics.Debug"/>）。</summary>
    public bool Debug { get; init; }

    /// <summary>是否将裁剪后的单元格图写入磁盘（需设置 <see cref="DebugOutputDirectory"/>）。</summary>
    public bool SaveCellDebugImages { get; init; }

    /// <summary>调试图片输出目录。</summary>
    public string? DebugOutputDirectory { get; init; }

    /// <summary>调试子目录/文件前缀名。</summary>
    public string DebugImageBaseName { get; init; } = "quicktable";
}
