using OpenCvSharp;

namespace Swg.OCR;

/// <summary>
/// 与 PaddleOCR 表格单元匹配一致的矩形工具（库内 <c>RectHelper</c> 不可见时本地实现）。
/// </summary>
internal static class OcrRectHelper
{
    /// <summary>各边外扩指定像素（左上角不小于 0）。</summary>
    public static Rect Extend(Rect r, int pixels)
    {
        return new Rect(
            Math.Max(0, r.X - pixels),
            Math.Max(0, r.Y - pixels),
            r.Width + 2 * pixels,
            r.Height + 2 * pixels);
    }

    /// <summary>交并比 IoU（Intersection over Union）。</summary>
    public static double IntersectionOverUnion(Rect a, Rect b)
    {
        int x1 = Math.Max(a.X, b.X);
        int y1 = Math.Max(a.Y, b.Y);
        int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
        int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);
        int w = Math.Max(0, x2 - x1);
        int h = Math.Max(0, y2 - y1);
        int inter = w * h;
        if (inter <= 0)
            return 0;
        int areaA = a.Width * a.Height;
        int areaB = b.Width * b.Height;
        return inter / (double)(areaA + areaB - inter);
    }

    /// <summary>两矩形中心点欧氏距离。</summary>
    public static double Distance(Rect a, Rect b)
    {
        double ax = a.X + a.Width * 0.5;
        double ay = a.Y + a.Height * 0.5;
        double bx = b.X + b.Width * 0.5;
        double by = b.Y + b.Height * 0.5;
        double dx = ax - bx;
        double dy = ay - by;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
