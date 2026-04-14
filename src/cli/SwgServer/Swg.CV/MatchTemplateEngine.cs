using OpenCvSharp;

namespace Swg.CV;

/// <summary>
/// 策略路由 + 金字塔粗定位 + 细窗口多尺度模板匹配（CCoeffNormed，灰度）。
/// </summary>
internal static class MatchTemplateEngine
{
    private const float ScaleDelta = 0.08f;

    /// <summary>
    /// 在 ROI 图像上匹配模板；返回的矩形为相对 ROI 左上角；屏幕坐标由调用方平移。
    /// </summary>
    public static bool TryMatch(Mat roiBgr, Mat templBgr, double threshold, out double score, out OpenCvSharp.Rect matchInRoi)
    {
        score = 0;
        matchInRoi = default;

        if (roiBgr.Empty() || templBgr.Empty())
            return false;

        if (templBgr.Width >= roiBgr.Width || templBgr.Height >= roiBgr.Height)
            return false;

        using var grayRoi = new Mat();
        using var grayTempl = new Mat();
        Cv2.CvtColor(roiBgr, grayRoi, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(templBgr, grayTempl, ColorConversionCodes.BGR2GRAY);

        int area = grayRoi.Width * grayRoi.Rows;
        bool strategyB = area > GdiScreenCapture.StrategyAreaThreshold;
        float s0 = GdiScreenCapture.GetSystemDpiScale();
        float[] scales = BuildScaleFactors(s0);

        double best = -1.0;
        OpenCvSharp.Rect bestR = default;
        int bestTw = 0;
        int bestTh = 0;

        if (strategyB)
            TryPyramidCoarseThenRefine(grayRoi, grayTempl, scales, ref best, ref bestR, ref bestTw, ref bestTh);

        if (best < threshold)
        {
            // 策略 A，或策略 B 粗窗未达标：整幅 ROI 多尺度（§9.4 回退）
            best = -1.0;
            bestR = default;
            bestTw = 0;
            bestTh = 0;
            MatchMultiScaleOnRegion(grayRoi, grayTempl, scales, 0, 0, ref best, ref bestR, ref bestTw, ref bestTh);
        }

        if (best < threshold || bestTw <= 0)
            return false;

        score = best;
        matchInRoi = new OpenCvSharp.Rect(bestR.X, bestR.Y, bestTw, bestTh);
        return true;
    }

    private static float[] BuildScaleFactors(float s0)
    {
        var set = new HashSet<float> { 1f, s0 };
        void add(float x)
        {
            if (x is >= 0.5f and <= 1.5f)
                set.Add(x);
        }

        add(s0 - ScaleDelta);
        add(s0 + ScaleDelta);
        add(s0 - 2 * ScaleDelta);
        add(s0 + 2 * ScaleDelta);
        return set.OrderBy(x => x).ToArray();
    }

    private static void TryPyramidCoarseThenRefine(
        Mat grayRoi,
        Mat grayTempl,
        float[] scales,
        ref double best,
        ref OpenCvSharp.Rect bestR,
        ref int bestTw,
        ref int bestTh)
    {
        using var l1Roi = new Mat();
        using var l2Roi = new Mat();
        Cv2.PyrDown(grayRoi, l1Roi);
        Cv2.PyrDown(l1Roi, l2Roi);

        using var t1 = new Mat();
        using var t2 = new Mat();
        Cv2.PyrDown(grayTempl, t1);
        Cv2.PyrDown(t1, t2);

        Mat coarseImg = l2Roi;
        Mat coarseTempl = t2;
        if (coarseTempl.Width < 4 || coarseTempl.Height < 4 || coarseTempl.Width >= coarseImg.Width || coarseTempl.Height >= coarseImg.Height)
        {
            coarseImg = l1Roi;
            coarseTempl = t1;
            if (coarseTempl.Width < 2 || coarseTempl.Height < 2 || coarseTempl.Width >= coarseImg.Width || coarseTempl.Height >= coarseImg.Height)
                return;
        }

        using var coarseResult = new Mat();
        Cv2.MatchTemplate(coarseImg, coarseTempl, coarseResult, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(coarseResult, out _, out double coarseMax, out _, out OpenCvSharp.Point maxLoc);

        double sx = grayRoi.Width / (double)Math.Max(1, coarseImg.Cols);
        double sy = grayRoi.Height / (double)Math.Max(1, coarseImg.Rows);
        int x0 = (int)(maxLoc.X * sx);
        int y0 = (int)(maxLoc.Y * sy);

        int winW = Math.Max(grayTempl.Cols * 4, 64);
        int winH = Math.Max(grayTempl.Rows * 4, 64);
        var win = CenteredWindow(x0, y0, winW, winH, grayRoi.Width, grayRoi.Height);
        using var sub = new Mat(grayRoi, win);

        best = -1;
        bestR = default;
        bestTw = 0;
        bestTh = 0;
        MatchMultiScaleOnRegion(sub, grayTempl, scales, win.X, win.Y, ref best, ref bestR, ref bestTw, ref bestTh);

        // 细窗口扩大 2 倍再试（§9.4）
        winW = Math.Min(grayRoi.Width, winW * 2);
        winH = Math.Min(grayRoi.Height, winH * 2);
        win = CenteredWindow(x0, y0, winW, winH, grayRoi.Width, grayRoi.Height);
        using (var sub2 = new Mat(grayRoi, win))
        {
            MatchMultiScaleOnRegion(sub2, grayTempl, scales, win.X, win.Y, ref best, ref bestR, ref bestTw, ref bestTh);
        }
    }

    private static OpenCvSharp.Rect CenteredWindow(int cx, int cy, int w, int h, int boundW, int boundH)
    {
        int x = cx - w / 2;
        int y = cy - h / 2;
        if (x < 0)
            x = 0;
        if (y < 0)
            y = 0;
        if (x + w > boundW)
            x = Math.Max(0, boundW - w);
        if (y + h > boundH)
            y = Math.Max(0, boundH - h);
        w = Math.Min(w, boundW - x);
        h = Math.Min(h, boundH - y);
        return new OpenCvSharp.Rect(x, y, w, h);
    }

    private static void MatchMultiScaleOnRegion(
        Mat regionGray,
        Mat templGray,
        float[] scales,
        int parentOffsetX,
        int parentOffsetY,
        ref double best,
        ref OpenCvSharp.Rect bestR,
        ref int bestTw,
        ref int bestTh)
    {
        foreach (float s in scales)
        {
            int tw = Math.Max(1, (int)Math.Round(templGray.Cols * s));
            int th = Math.Max(1, (int)Math.Round(templGray.Rows * s));
            if (tw < 2 || th < 2 || tw > regionGray.Width || th > regionGray.Height)
                continue;

            using var scaledT = new Mat();
            Cv2.Resize(templGray, scaledT, new OpenCvSharp.Size(tw, th));

            using var result = new Mat();
            Cv2.MatchTemplate(regionGray, scaledT, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

            if (maxVal > best)
            {
                best = maxVal;
                int absX = parentOffsetX + maxLoc.X;
                int absY = parentOffsetY + maxLoc.Y;
                bestR = new OpenCvSharp.Rect(absX, absY, tw, th);
                bestTw = tw;
                bestTh = th;
            }
        }
    }
}
