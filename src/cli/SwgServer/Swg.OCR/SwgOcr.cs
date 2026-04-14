using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;
using Swg.CV;
using Swg.OCR.QuickTable;
using Tesseract;

namespace Swg.OCR;

/// <summary>
/// OCR 静态入口：双引擎（PaddleSharp / Tesseract）、屏幕 ROI 与图片、表格（Paddle 或 QuickTable 线网+Tesseract）。
/// </summary>
public static class SwgOcr
{
    private static readonly object Gate = new();

    private static PaddleOcrAll? _paddleZhV3;
    private static PaddleOcrAll? _paddleZhV4;
    private static PaddleOcrAll? _paddleZhV5;
    private static PaddleOcrAll? _paddleEn;
    private static PaddleOcrTableRecognizer? _tableRecognizer;
    private static TesseractEngine? _tesseractChinese;
    private static TesseractEngine? _tesseractEnglish;

    /// <summary>屏幕 ROI 内识别全部文字（坐标为相对屏幕左上角）。</summary>
    public static IReadOnlyList<OcrStringLineResult> RecognizeScreenStrings(ScreenRoi roi, OcrOptions options = default)
    {
        if (!roi.IsValid)
            throw new ArgumentException("ROI 无效（宽高须为正）。", nameof(roi));

        using Mat? mat = SwgCv.CaptureRegionToMat(roi);
        if (mat is null || mat.Empty())
            throw new InvalidOperationException("区域截屏失败（越界或 GDI 错误）。");

        return RunStringOcr(mat, roi, options);
    }

    /// <summary>
    /// 屏幕 ROI 内识别后筛选：<paramref name="matchText"/> 不得 Trim；
    /// 仅保留识别 <c>Text</c> 使用 Ordinal 包含该原始子串的项。
    /// </summary>
    public static IReadOnlyList<OcrStringLineResult> RecognizeScreenStringsContaining(
        ScreenRoi roi,
        string matchText,
        OcrOptions options = default)
    {
        if (matchText is null)
            throw new ArgumentNullException(nameof(matchText));
        if (matchText.Length == 0)
            throw new ArgumentException("matchText 不能为空字符串。", nameof(matchText));

        IReadOnlyList<OcrStringLineResult> all = RecognizeScreenStrings(roi, options);
        return FilterContains(all, matchText);
    }

    /// <summary>屏幕 ROI 内表格识别（仅 Paddle）；坐标相对屏幕左上角。</summary>
    public static IReadOnlyList<OcrTableCellResult> RecognizeScreenTable(ScreenRoi roi, OcrOptions options = default)
    {
        EnsurePaddleTable(options);

        if (!roi.IsValid)
            throw new ArgumentException("ROI 无效（宽高须为正）。", nameof(roi));

        using Mat? mat = SwgCv.CaptureRegionToMat(roi);
        if (mat is null || mat.Empty())
            throw new InvalidOperationException("区域截屏失败（越界或 GDI 错误）。");

        return RunTableOcr(mat, roi, options);
    }

    /// <summary>从图片（路径或 Data URL / Base64）识别全部文字；坐标相对原图左上角。</summary>
    public static IReadOnlyList<OcrStringLineResult> RecognizeImageStrings(string imagePathOrBase64, OcrOptions options = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imagePathOrBase64);
        using Mat? mat = TemplateDecoder.Decode(imagePathOrBase64);
        if (mat is null || mat.Empty())
            throw new ArgumentException("无法解码图片（路径无效或 Base64/Data URL 无法解析为图像）。", nameof(imagePathOrBase64));

        return RunStringOcr(mat, null, options);
    }

    /// <summary>从图片识别表格（仅 Paddle）。</summary>
    public static IReadOnlyList<OcrTableCellResult> RecognizeImageTable(string imagePathOrBase64, OcrOptions options = default)
    {
        EnsurePaddleTable(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(imagePathOrBase64);
        using Mat? mat = TemplateDecoder.Decode(imagePathOrBase64);
        if (mat is null || mat.Empty())
            throw new ArgumentException("无法解码图片（路径无效或 Base64/Data URL 无法解析为图像）。", nameof(imagePathOrBase64));

        return RunTableOcr(mat, null, options);
    }

    /// <summary>
    /// 屏幕 ROI 内「快速表格」：线检测网格 + Tesseract 逐格 OCR；坐标相对屏幕左上角。
    /// </summary>
    /// <param name="roi">屏幕区域。</param>
    /// <param name="language">Tesseract 语言。</param>
    public static IReadOnlyList<OcrTableCellResult> RecognizeScreenQuickTable(ScreenRoi roi, OcrLanguage language = OcrLanguage.Chinese) =>
        RecognizeScreenQuickTable(roi, new QuickTableDetectOptions { Language = language });

    /// <summary>
    /// 屏幕 ROI 内快速表格（可配置调试落盘等）。
    /// </summary>
    public static IReadOnlyList<OcrTableCellResult> RecognizeScreenQuickTable(ScreenRoi roi, QuickTableDetectOptions quickOptions)
    {
        ArgumentNullException.ThrowIfNull(quickOptions);
        if (!roi.IsValid)
            throw new ArgumentException("ROI 无效（宽高须为正）。", nameof(roi));

        using Mat? mat = SwgCv.CaptureRegionToMat(roi);
        if (mat is null || mat.Empty())
            throw new InvalidOperationException("区域截屏失败（越界或 GDI 错误）。");

        var detector = new QuickTableDetector();
        QuickTableDetectionResult result = detector.Detect(mat, quickOptions);
        return MapQuickTableCells(result.Cells, roi.Left, roi.Top);
    }

    /// <summary>从图片快速表格识别（线网 + Tesseract）；坐标相对原图左上角。</summary>
    public static IReadOnlyList<OcrTableCellResult> RecognizeImageQuickTable(
        string imagePathOrBase64,
        OcrLanguage language = OcrLanguage.Chinese) =>
        RecognizeImageQuickTable(imagePathOrBase64, new QuickTableDetectOptions { Language = language });

    /// <summary>从图片快速表格识别（可配置调试等）。</summary>
    public static IReadOnlyList<OcrTableCellResult> RecognizeImageQuickTable(
        string imagePathOrBase64,
        QuickTableDetectOptions quickOptions)
    {
        ArgumentNullException.ThrowIfNull(quickOptions);
        ArgumentException.ThrowIfNullOrWhiteSpace(imagePathOrBase64);
        using Mat? mat = TemplateDecoder.Decode(imagePathOrBase64);
        if (mat is null || mat.Empty())
            throw new ArgumentException("无法解码图片（路径无效或 Base64/Data URL 无法解析为图像）。", nameof(imagePathOrBase64));

        var detector = new QuickTableDetector();
        QuickTableDetectionResult result = detector.Detect(mat, quickOptions);
        return MapQuickTableCells(result.Cells, 0, 0);
    }

    private static IReadOnlyList<OcrTableCellResult> MapQuickTableCells(
        IReadOnlyList<QuickTableCell> cells,
        int offsetX,
        int offsetY) =>
        cells.Select(c => new OcrTableCellResult(
            c.Text,
            offsetX + c.X1,
            offsetY + c.Y1,
            c.X2 - c.X1,
            c.Y2 - c.Y1,
            c.Row,
            c.Col)).ToList();

    private static IReadOnlyList<OcrStringLineResult> FilterContains(IReadOnlyList<OcrStringLineResult> all, string matchText)
    {
        var list = new List<OcrStringLineResult>();
        foreach (OcrStringLineResult line in all)
        {
            if (line.Text.Contains(matchText, StringComparison.Ordinal))
                list.Add(line);
        }

        return list;
    }

    private static void EnsurePaddleTable(OcrOptions options)
    {
        if (options.Engine != OcrEngineKind.PaddleSharp)
            throw new ArgumentException("表格识别仅支持 PaddleSharp 引擎。", nameof(options));
    }

    private static IReadOnlyList<OcrStringLineResult> RunStringOcr(Mat mat, ScreenRoi? screenOffset, OcrOptions options)
    {
        int ox = screenOffset?.Left ?? 0;
        int oy = screenOffset?.Top ?? 0;

        return options.Engine switch
        {
            OcrEngineKind.PaddleSharp => RunPaddleString(mat, ox, oy, options),
            OcrEngineKind.Tesseract => RunTesseractString(mat, ox, oy, options),
            _ => throw new ArgumentOutOfRangeException(nameof(options)),
        };
    }

    private static IReadOnlyList<OcrStringLineResult> RunPaddleString(Mat mat, int offsetX, int offsetY, OcrOptions options)
    {
        PaddleOcrAll all = GetPaddleAll(options);
        PaddleOcrResult result;
        lock (Gate)
            result = all.Run(mat);

        var list = new List<OcrStringLineResult>(result.Regions.Length);
        foreach (PaddleOcrResultRegion region in result.Regions)
        {
            OpenCvSharp.Rect r = region.Rect.BoundingRect();
            double cx = offsetX + r.X + r.Width * 0.5;
            double cy = offsetY + r.Y + r.Height * 0.5;
            list.Add(new OcrStringLineResult(
                region.Text,
                offsetX + r.X,
                offsetY + r.Y,
                r.Width,
                r.Height,
                cx,
                cy,
                region.Score));
        }

        return list;
    }

    private static IReadOnlyList<OcrStringLineResult> RunTesseractString(Mat mat, int offsetX, int offsetY, OcrOptions options)
    {
        TesseractEngine engine = GetTesseract(options.Language);
        if (!Cv2.ImEncode(".png", mat, out byte[] png) || png.Length == 0)
            throw new InvalidOperationException("Tesseract 前图像编码为 PNG 失败。");

        using var pix = Pix.LoadFromMemory(png);
        using Page page = engine.Process(pix);
        var list = new List<OcrStringLineResult>();
        using ResultIterator iter = page.GetIterator();
        iter.Begin();
        do
        {
            string? text = iter.GetText(PageIteratorLevel.Word);
            if (string.IsNullOrEmpty(text))
                continue;

            if (!iter.TryGetBoundingBox(PageIteratorLevel.Word, out Tesseract.Rect tb))
                continue;

            float conf = iter.GetConfidence(PageIteratorLevel.Word);
            double conf01 = conf <= 0 ? 0 : Math.Clamp(conf / 100.0, 0, 1);
            int left = offsetX + tb.X1;
            int top = offsetY + tb.Y1;
            int w = tb.Width;
            int h = tb.Height;
            double cx = left + w * 0.5;
            double cy = top + h * 0.5;
            list.Add(new OcrStringLineResult(text, left, top, w, h, cx, cy, conf01));
        }
        while (iter.Next(PageIteratorLevel.Word));

        return list;
    }

    private static IReadOnlyList<OcrTableCellResult> RunTableOcr(Mat mat, ScreenRoi? screenOffset, OcrOptions options)
    {
        int ox = screenOffset?.Left ?? 0;
        int oy = screenOffset?.Top ?? 0;

        TableDetectionResult tableResult;
        PaddleOcrResult ocrResult;
        PaddleOcrAll all = GetPaddleAll(options);
        lock (Gate)
        {
            tableResult = GetTableRecognizer().Run(mat);
            ocrResult = all.Run(mat);
        }

        var matched = new List<string>[tableResult.StructureBoxes.Count];
        for (int i = 0; i < matched.Length; i++)
            matched[i] = [];

        for (int i = 0; i < ocrResult.Regions.Length; i++)
        {
            PaddleOcrResultRegion region = ocrResult.Regions[i];
            OpenCvSharp.Rect ocrBox = OcrRectHelper.Extend(region.Rect.BoundingRect(), 1);
            int best = tableResult.StructureBoxes
                .Select((box, si) => new
                {
                    IouScore = OcrRectHelper.IntersectionOverUnion(ocrBox, box.Rect),
                    DistanceScore = OcrRectHelper.Distance(ocrBox, box.Rect),
                    Index = si,
                })
                .OrderByDescending(x => x.IouScore)
                .ThenBy(x => x.DistanceScore)
                .First()
                .Index;
            matched[best].Add(region.Text);
        }

        var rawCells = new List<(OpenCvSharp.Rect Bbox, string Text)>(tableResult.StructureBoxes.Count);
        for (int si = 0; si < tableResult.StructureBoxes.Count; si++)
        {
            OpenCvSharp.Rect r = tableResult.StructureBoxes[si].Rect;
            string cellText = string.Join(" ", matched[si]);
            rawCells.Add((r, cellText));
        }

        IReadOnlyList<(int Row, int Col, OpenCvSharp.Rect R, string T)> grid = TableGridBuilder.AssignRowCol(rawCells);
        var output = new List<OcrTableCellResult>(grid.Count);
        foreach (var g in grid)
        {
            output.Add(new OcrTableCellResult(
                g.T,
                ox + g.R.X,
                oy + g.R.Y,
                g.R.Width,
                g.R.Height,
                g.Row,
                g.Col));
        }

        return output;
    }

    private static PaddleOcrAll GetPaddleAll(OcrOptions options)
    {
        lock (Gate)
        {
            if (options.Language == OcrLanguage.English)
            {
                return _paddleEn ??= new PaddleOcrAll(LocalFullModels.EnglishV3, PaddleDevice.Mkldnn());
            }

            return options.PaddleChineseModel switch
            {
                PaddleChineseModelVersion.V4 => _paddleZhV4 ??= new PaddleOcrAll(LocalFullModels.ChineseV4, PaddleDevice.Mkldnn()),
                PaddleChineseModelVersion.V5 => _paddleZhV5 ??= new PaddleOcrAll(LocalFullModels.ChineseV5, PaddleDevice.Mkldnn()),
                _ => _paddleZhV3 ??= new PaddleOcrAll(LocalFullModels.ChineseV3, PaddleDevice.Mkldnn()),
            };
        }
    }

    private static PaddleOcrTableRecognizer GetTableRecognizer()
    {
        lock (Gate)
        {
            return _tableRecognizer ??= new PaddleOcrTableRecognizer(LocalTableRecognitionModel.ChineseMobileV2_SLANET);
        }
    }

    private static TesseractEngine GetTesseract(OcrLanguage language)
    {
        string lang = language == OcrLanguage.English ? "eng" : "chi_sim";
        lock (Gate)
        {
            if (language == OcrLanguage.English)
            {
                if (_tesseractEnglish is null)
                    _tesseractEnglish = CreateTesseractEngine(lang);
                return _tesseractEnglish;
            }

            if (_tesseractChinese is null)
                _tesseractChinese = CreateTesseractEngine(lang);
            return _tesseractChinese;
        }
    }

    private static TesseractEngine CreateTesseractEngine(string language)
    {
        string baseDir = AppContext.BaseDirectory;
        string tessDir = Path.Combine(baseDir, "tessdata");
        if (!Directory.Exists(tessDir))
            throw new InvalidOperationException($"未找到 Tesseract 语言数据目录: {tessDir}（请放置 .traineddata 并确保已复制到输出目录）。");

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
