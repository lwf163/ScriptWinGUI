using OpenCvSharp;

namespace Swg.OCR.QuickTable;

/// <summary>裁剪单元格并做 OCR 前增强。</summary>
public sealed class QuickTableCellCropper
{
    /// <summary>裁剪单格并返回增强后的二值图（调用方负责 <c>Dispose</c>）。</summary>
    public Mat CropCell(Mat img, QuickTableCell bbox, int pad = 0)
    {
        int h = img.Rows;
        int w = img.Cols;

        int x1 = Math.Max(0, bbox.X1 - pad);
        int y1 = Math.Max(0, bbox.Y1 - pad);
        int x2 = Math.Min(w, bbox.X2 + pad);
        int y2 = Math.Min(h, bbox.Y2 + pad);

        var roi = new Rect(x1, y1, x2 - x1, y2 - y1);
        using var cropped = new Mat(img, roi);

        const int cropPixels = 2;
        int newWidth = Math.Max(1, cropped.Width - 2 * cropPixels);
        int newHeight = Math.Max(1, cropped.Height - 2 * cropPixels);
        var cropRoi = new Rect(cropPixels, cropPixels, newWidth, newHeight);
        using var inner = new Mat(cropped, cropRoi);

        return EnhanceForOcr(inner);
    }

    private static Mat EnhanceForOcr(Mat img)
    {
        using Mat gray = img.Channels() == 3
            ? new Mat()
            : img.Clone();
        if (img.Channels() == 3)
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);

        using var kernel = new Mat(3, 3, MatType.CV_32F);
        kernel.Set<float>(0, 0, 0);
        kernel.Set<float>(0, 1, -1);
        kernel.Set<float>(0, 2, 0);
        kernel.Set<float>(1, 0, -1);
        kernel.Set<float>(1, 1, 5);
        kernel.Set<float>(1, 2, -1);
        kernel.Set<float>(2, 0, 0);
        kernel.Set<float>(2, 1, -1);
        kernel.Set<float>(2, 2, 0);

        using var sharpened = new Mat();
        Cv2.Filter2D(gray, sharpened, MatType.CV_8UC1, kernel);

        var binary = new Mat();
        Cv2.AdaptiveThreshold(
            sharpened,
            binary,
            255,
            AdaptiveThresholdTypes.GaussianC,
            ThresholdTypes.Binary,
            11,
            2);

        return binary;
    }

    /// <summary>将单元格裁剪图写入目录（调试）。</summary>
    public List<string> SaveCells(
        Mat img,
        List<QuickTableCell> cells,
        string outDir,
        string imageName,
        int pad = 0,
        string format = "png",
        string prefix = "cell")
    {
        if (cells is null || cells.Count == 0)
            return [];

        Directory.CreateDirectory(outDir);
        var cellsSorted = cells.OrderBy(c => c.Row).ThenBy(c => c.Col).ToList();
        var savedPaths = new List<string>();

        foreach (QuickTableCell cell in cellsSorted)
        {
            using Mat? crop = CropCell(img, cell, pad);
            string filename = $"{prefix}_r{cell.Row:D3}_c{cell.Col:D3}.{format}";
            string outPath = Path.Combine(outDir, filename);
            Cv2.ImWrite(outPath, crop);
            savedPaths.Add(outPath);
        }

        return savedPaths;
    }
}
