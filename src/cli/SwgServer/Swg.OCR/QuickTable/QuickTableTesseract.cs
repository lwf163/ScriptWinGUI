using Tesseract;

namespace Swg.OCR.QuickTable;

/// <summary>QuickTable 专用 Tesseract 引擎懒加载（与 <see cref="SwgOcr"/> 使用相同 tessdata 路径约定）。</summary>
internal static class QuickTableTesseract
{
    private static readonly object Gate = new();
    private static TesseractEngine? _chinese;
    private static TesseractEngine? _english;

    /// <summary>按语言获取共享引擎。</summary>
    public static TesseractEngine GetEngine(OcrLanguage language)
    {
        lock (Gate)
        {
            if (language == OcrLanguage.English)
            {
                return _english ??= CreateEngine("eng");
            }

            return _chinese ??= CreateEngine("chi_sim");
        }
    }

    private static TesseractEngine CreateEngine(string language)
    {
        string baseDir = AppContext.BaseDirectory;
        string tessDir = Path.Combine(baseDir, "tessdata");
        if (!Directory.Exists(tessDir))
        {
            throw new InvalidOperationException(
                $"未找到 Tesseract 语言数据目录: {tessDir}（请放置 .traineddata 并确保已复制到输出目录）。");
        }

        try
        {
            return new TesseractEngine(baseDir, language, EngineMode.Default);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"初始化 Tesseract 引擎失败（语言: {language}）。", ex);
        }
    }
}
