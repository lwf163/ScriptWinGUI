using OpenCvSharp;
using Serilog;
using TesseractEngine = Tesseract.TesseractEngine;
using TesseractPage = Tesseract.Page;
using TesseractPix = Tesseract.Pix;

namespace Swg.OCR.QuickTable;

/// <summary>
/// 基于线检测网格 + 逐格 Tesseract 的快速表格识别（适合有明显表格线的图像）。
/// </summary>
public sealed class QuickTableDetector
{
    private static readonly ILogger Logger = Log.ForContext(typeof(QuickTableDetector));

    private readonly QuickTableImageProcessor _imageProcessor = new();
    private readonly QuickTableLineDetector _lineDetector = new();
    private readonly QuickTableLineMerger _lineMerger = new();
    private readonly QuickTableIntersectionFinder _intersectionFinder = new();
    private readonly QuickTableCoordinateClusterer _coordinateClusterer = new();
    private readonly QuickTableCellGenerator _cellGenerator = new();
    private readonly QuickTableCellCropper _cellCropper = new();

    /// <summary>检测表格并对单元格执行 Tesseract OCR。</summary>
    /// <param name="image">BGR 或灰度图。</param>
    /// <param name="options">选项；null 表示默认。</param>
    public QuickTableDetectionResult Detect(Mat image, QuickTableDetectOptions? options = null)
    {
        options ??= new QuickTableDetectOptions();
        bool dbg = options.Debug;

        using Mat binImg = _imageProcessor.PreprocessImage(image);

        (List<QuickTableLine> hLines, List<QuickTableLine> vLines) = _lineDetector.DetectLines(binImg);
        List<QuickTableLine> hLinesM = _lineMerger.MergeLines(hLines, "h");
        List<QuickTableLine> vLinesM = _lineMerger.MergeLines(vLines, "v");

        if (dbg)
        {
            Logger.Debug(
                "QuickTable 线数量: 水平 {HBefore}->{HAfter}，垂直 {VBefore}->{VAfter}",
                hLines.Count,
                hLinesM.Count,
                vLines.Count,
                vLinesM.Count);
        }

        List<QuickTablePoint> pts = _intersectionFinder.FindIntersections(hLinesM, vLinesM);
        if (pts.Count == 0)
        {
            return new QuickTableDetectionResult
            {
                Rows = [],
                Cols = [],
                Cells = [],
            };
        }

        List<int> rowCoords = _coordinateClusterer.ClusterCoordinates(pts, "y");
        List<int> colCoords = _coordinateClusterer.ClusterCoordinates(pts, "x");

        int imgHeight = image.Rows;
        int imgWidth = image.Cols;

        if (!rowCoords.Any(y => y <= 10))
            rowCoords.Insert(0, 0);
        if (!rowCoords.Any(y => Math.Abs(y - imgHeight) <= 10))
            rowCoords.Add(imgHeight);
        if (!colCoords.Any(x => x <= 10))
            colCoords.Insert(0, 0);
        if (!colCoords.Any(x => Math.Abs(x - imgWidth) <= 10))
            colCoords.Add(imgWidth);

        if (dbg)
        {
            Logger.Debug("QuickTable 行坐标: {RowCoords}", string.Join(", ", rowCoords));
            Logger.Debug("QuickTable 列坐标: {ColCoords}", string.Join(", ", colCoords));
        }

        List<QuickTableCell> cells = _cellGenerator.GenerateCells(rowCoords, colCoords);
        List<QuickTableCell> filteredCells = FilterCellsByRowHeight(cells, dbg);

        if (options.SaveCellDebugImages && !string.IsNullOrWhiteSpace(options.DebugOutputDirectory))
        {
            string sub = Path.Combine(options.DebugOutputDirectory, options.DebugImageBaseName);
            Directory.CreateDirectory(sub);
            _cellCropper.SaveCells(image, filteredCells, sub, options.DebugImageBaseName, pad: 0, format: "png");
        }

        PerformOcrOnCells(image, filteredCells, options.Language, dbg);

        return new QuickTableDetectionResult
        {
            Rows = rowCoords,
            Cols = colCoords,
            Cells = filteredCells,
        };
    }

    private List<QuickTableCell> FilterCellsByRowHeight(List<QuickTableCell> cells, bool dbg)
    {
        if (cells.Count == 0)
            return cells;

        var rows = cells.GroupBy(c => c.Row)
            .Select(g => new
            {
                RowNum = g.Key,
                Height = g.Max(c => c.Y2) - g.Min(c => c.Y1),
                Cells = g.ToList(),
            })
            .OrderBy(r => r.RowNum)
            .ToList();

        if (rows.Count == 0)
            return cells;

        var heights = rows.Select(r => r.Height).ToList();
        double avgHeight = heights.Average();
        double variance = heights.Sum(h => Math.Pow(h - avgHeight, 2)) / heights.Count;
        double stdDev = Math.Sqrt(variance);

        if (dbg)
        {
            Logger.Debug("QuickTable 平均行高 {AvgHeight:F2}，标准差 {StdDev:F2}", avgHeight, stdDev);
        }

        int endRow = -1;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].Height > avgHeight * 2.0 && i > rows.Count / 2)
            {
                endRow = rows[i].RowNum;
                if (dbg)
                {
                    Logger.Debug(
                        "QuickTable 检测到异常行 {EndRow}，高度 {Height}，截断后续行",
                        endRow,
                        rows[i].Height);
                }

                break;
            }
        }

        return endRow >= 0 ? cells.Where(c => c.Row < endRow).ToList() : cells;
    }

    private static void PerformOcrOnCells(Mat img, List<QuickTableCell> cells, OcrLanguage language, bool dbg)
    {
        TesseractEngine engine = QuickTableTesseract.GetEngine(language);
        int count = 0;

        foreach (QuickTableCell cell in cells)
        {
            var rect = new OpenCvSharp.Rect(cell.X1, cell.Y1, cell.X2 - cell.X1, cell.Y2 - cell.Y1);
            rect.X = Math.Max(0, rect.X);
            rect.Y = Math.Max(0, rect.Y);
            rect.Width = Math.Min(img.Width - rect.X, rect.Width);
            rect.Height = Math.Min(img.Height - rect.Y, rect.Height);

            if (rect.Width <= 0 || rect.Height <= 0)
            {
                cell.Text = string.Empty;
                continue;
            }

            using var cellRoi = new Mat(img, rect);
            if (!Cv2.ImEncode(".png", cellRoi, out byte[] png) || png.Length == 0)
            {
                cell.Text = string.Empty;
                continue;
            }

            using var pix = TesseractPix.LoadFromMemory(png);
            using TesseractPage page = engine.Process(pix);
            cell.Text = page.GetText().Trim();

            count++;
            if (dbg && count % 50 == 0)
                Logger.Debug("QuickTable OCR 进度 {Done}/{Total}", count, cells.Count);
        }
    }
}
