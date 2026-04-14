using OpenCvSharp;

namespace Swg.OCR.QuickTable;

/// <summary>形态学 + 霍夫线检测水平/垂直表格线。</summary>
public sealed class QuickTableLineDetector
{
    /// <summary>从二值图检测水平线与垂直线。</summary>
    public (List<QuickTableLine> HorizontalLines, List<QuickTableLine> VerticalLines) DetectLines(
        Mat binImg,
        double minLenRatio = 0.1,
        int minGap = 10)
    {
        int h = binImg.Rows;
        int w = binImg.Cols;

        using var horizontal = binImg.Clone();
        using var horizKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(Math.Max(3, w / 20), 1));
        Cv2.MorphologyEx(horizontal, horizontal, MorphTypes.Open, horizKernel, null, 1);

        using var vertical = binImg.Clone();
        using var vertKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, Math.Max(3, h / 20)));
        Cv2.MorphologyEx(vertical, vertical, MorphTypes.Open, vertKernel, null, 1);

        double rho = 1;
        double theta = Math.PI / 180;
        int threshold = Math.Max(50, (int)(Math.Min(h, w) / 50));
        int minLineLengthH = (int)(w * minLenRatio);
        int minLineLengthV = (int)(h * minLenRatio);
        int maxLineGap = minGap;

        var hLines = new List<QuickTableLine>();
        var vLines = new List<QuickTableLine>();

        LineSegmentPoint[]? linesH = Cv2.HoughLinesP(horizontal, rho, theta, threshold, minLineLengthH, maxLineGap);
        LineSegmentPoint[]? linesV = Cv2.HoughLinesP(vertical, rho, theta, threshold, minLineLengthV, maxLineGap);

        if (linesH is not null)
        {
            foreach (LineSegmentPoint line in linesH)
            {
                int x1 = line.P1.X, y1 = line.P1.Y, x2 = line.P2.X, y2 = line.P2.Y;
                if (Math.Abs(y2 - y1) <= 2)
                    hLines.Add(new QuickTableLine(x1, y1, x2, y2));
            }
        }

        if (linesV is not null)
        {
            foreach (LineSegmentPoint line in linesV)
            {
                int x1 = line.P1.X, y1 = line.P1.Y, x2 = line.P2.X, y2 = line.P2.Y;
                if (Math.Abs(x2 - x1) <= 2)
                    vLines.Add(new QuickTableLine(x1, y1, x2, y2));
            }
        }

        return (hLines, vLines);
    }
}
