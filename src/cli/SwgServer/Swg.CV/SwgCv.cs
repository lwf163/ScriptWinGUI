using OpenCvSharp;

namespace Swg.CV;

/// <summary>
/// 屏幕 ROI 找图、颜色与截图的静态入口；截屏均经 Win32 GDI（BitBlt），与 <c>docs/requirements/cv.md</c> 一致。
/// </summary>
public static class SwgCv
{
    /// <summary>
    /// 通过 Win32 枚举顶层窗口，按窗口属性查找首个匹配项，并返回其对应的屏幕 ROI（左上 + 长宽）。
    /// </summary>
    /// <param name="windowTitleContains">窗口标题包含文本（可空；空表示不参与过滤）。</param>
    /// <param name="classNameEquals">窗口类名完全匹配文本（可空；空表示不参与过滤）。</param>
    /// <param name="processId">进程 ID（可空；空表示不参与过滤）。</param>
    /// <param name="visibleOnly">是否仅匹配可见窗口。</param>
    /// <param name="roi">成功时返回窗口矩形对应的 ROI；失败时为默认值。</param>
    /// <returns>找到匹配窗口且窗口矩形有效时返回 <c>true</c>，否则返回 <c>false</c>。</returns>
    public static bool TryFindWindowRoi(
        string? windowTitleContains,
        string? classNameEquals,
        int? processId,
        bool visibleOnly,
        out ScreenRoi roi)
    {
        roi = default;
        string? titleFilter = string.IsNullOrWhiteSpace(windowTitleContains) ? null : windowTitleContains;
        string? classFilter = string.IsNullOrWhiteSpace(classNameEquals) ? null : classNameEquals;

        nint matched = 0;
        Win32Native.EnumWindows((hWnd, _) =>
        {
            if (visibleOnly && !Win32Native.IsWindowVisible(hWnd))
                return true;
            if (processId.HasValue)
            {
                Win32Native.GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid != processId.Value)
                    return true;
            }

            if (classFilter is not null)
            {
                string cls = GetWindowClassName(hWnd);
                if (!string.Equals(cls, classFilter, StringComparison.Ordinal))
                    return true;
            }

            if (titleFilter is not null)
            {
                string title = GetWindowTitle(hWnd);
                if (title.IndexOf(titleFilter, StringComparison.OrdinalIgnoreCase) < 0)
                    return true;
            }

            matched = hWnd;
            return false;
        }, 0);

        if (matched == 0 || !Win32Native.GetWindowRect(matched, out var rect))
            return false;

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0)
            return false;

        roi = new ScreenRoi(rect.Left, rect.Top, width, height);
        return true;
    }

    private static string GetWindowTitle(nint hWnd)
    {
        int len = Win32Native.GetWindowTextLength(hWnd);
        var sb = new System.Text.StringBuilder(Math.Max(1, len + 1));
        _ = Win32Native.GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static string GetWindowClassName(nint hWnd)
    {
        var sb = new System.Text.StringBuilder(256);
        _ = Win32Native.GetClassName(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    /// <summary>
    /// 检查窗口矩形（<c>GetWindowRect</c>）与 Win32 截屏结果的像素尺寸是否一致，用于排查 DPI/缩放导致的坐标不一致问题。
    /// </summary>
    /// <param name="windowHandle">窗口句柄（HWND，建议传 16 进制字符串转后的值）。</param>
    /// <returns>包含 ROI、截图尺寸、窗口 DPI 与是否匹配的结果。</returns>
    public static WindowRoiConsistencyResult CheckWindowRoiCaptureConsistency(nint windowHandle)
    {
        if (windowHandle == 0 || !Win32Native.IsWindow(windowHandle))
            return new WindowRoiConsistencyResult(false, default, 0, 0, 0, false);

        if (!Win32Native.GetWindowRect(windowHandle, out var rect))
            return new WindowRoiConsistencyResult(false, default, 0, 0, 0, false);

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0)
            return new WindowRoiConsistencyResult(false, default, 0, 0, 0, false);

        var roi = new ScreenRoi(rect.Left, rect.Top, width, height);
        uint dpi = Win32Native.GetDpiForWindow(windowHandle);
        using Mat? mat = GdiScreenCapture.Capture(roi);
        if (mat is null || mat.Empty())
            return new WindowRoiConsistencyResult(false, roi, 0, 0, dpi, false);

        bool matched = mat.Width == roi.Width && mat.Height == roi.Height;
        return new WindowRoiConsistencyResult(true, roi, mat.Width, mat.Height, dpi, matched);
    }

    /// <summary>
    /// 在屏幕 ROI 内按相似度查找单个模板（路径、Data URL 或裸 Base64）。
    /// </summary>
    /// <param name="roi">屏幕 ROI：左上角 + 宽高（逻辑像素），须完全落在虚拟桌面内。</param>
    /// <param name="templatePathOrBase64">模板图像：本地文件路径，或 <c>data:image/...;base64,</c> 前缀的字符串，或裸 Base64 图像字节。</param>
    /// <param name="threshold">相似度阈值，与 <see cref="TemplateMatchModes.CCoeffNormed"/> 配套（越大越相似，通常约 0.85～0.95）。</param>
    /// <returns>命中时 <see cref="FindImageResult.Found"/> 为 <c>true</c>，矩形为屏幕绝对坐标；未命中或截屏/解码失败时为 <c>false</c>。</returns>
    public static FindImageResult FindSingleTemplate(ScreenRoi roi, string templatePathOrBase64, double threshold)
    {
        if (!roi.IsValid)
            return new FindImageResult(false, 0, 0, 0, 0, 0);

        using Mat? roiMat = GdiScreenCapture.Capture(roi);
        if (roiMat is null)
            return new FindImageResult(false, 0, 0, 0, 0, 0);

        using Mat? templ = TemplateDecoder.Decode(templatePathOrBase64);
        if (templ is null || templ.Empty())
            return new FindImageResult(false, 0, 0, 0, 0, 0);

        if (!MatchTemplateEngine.TryMatch(roiMat, templ, threshold, out double score, out OpenCvSharp.Rect r))
            return new FindImageResult(false, 0, 0, 0, 0, 0);

        return new FindImageResult(true, score, roi.Left + r.X, roi.Top + r.Y, r.Width, r.Height);
    }

    /// <summary>
    /// 多个模板中按 <paramref name="preference"/> 返回其一；同一 ROI 只截屏一次。
    /// </summary>
    /// <param name="roi">屏幕 ROI，含义同 <see cref="FindSingleTemplate"/>。</param>
    /// <param name="templatePathOrBase64List">模板列表，每项为路径或 Base64，规则同 <see cref="FindSingleTemplate"/>。</param>
    /// <param name="threshold">相似度阈值，含义同 <see cref="FindSingleTemplate"/>。</param>
    /// <param name="preference"><see cref="TemplateMatchPreference.FirstQualified"/> 时按列表顺序首个达标即返回；<see cref="TemplateMatchPreference.BestScore"/> 时在达标项中取最高分。</param>
    /// <returns>命中时 <see cref="FindOneOfTemplatesResult.Found"/> 为 <c>true</c> 且含模板下标与屏幕绝对矩形；否则 <c>false</c> 且下标为 -1。</returns>
    public static FindOneOfTemplatesResult FindOneOfTemplates(
        ScreenRoi roi,
        IReadOnlyList<string> templatePathOrBase64List,
        double threshold,
        TemplateMatchPreference preference)
    {
        if (!roi.IsValid || templatePathOrBase64List.Count == 0)
            return new FindOneOfTemplatesResult(false, -1, 0, 0, 0, 0, 0);

        using Mat? roiMat = GdiScreenCapture.Capture(roi);
        if (roiMat is null)
            return new FindOneOfTemplatesResult(false, -1, 0, 0, 0, 0, 0);

        if (preference == TemplateMatchPreference.FirstQualified)
        {
            for (int i = 0; i < templatePathOrBase64List.Count; i++)
            {
                using Mat? templ = TemplateDecoder.Decode(templatePathOrBase64List[i]);
                if (templ is null || templ.Empty())
                    continue;
                if (!MatchTemplateEngine.TryMatch(roiMat, templ, threshold, out double score, out OpenCvSharp.Rect r))
                    continue;
                return new FindOneOfTemplatesResult(true, i, score, roi.Left + r.X, roi.Top + r.Y, r.Width, r.Height);
            }

            return new FindOneOfTemplatesResult(false, -1, 0, 0, 0, 0, 0);
        }

        // BestScore
        double bestScore = -1;
        int bestIdx = -1;
        OpenCvSharp.Rect bestR = default;
        for (int i = 0; i < templatePathOrBase64List.Count; i++)
        {
            using Mat? templ = TemplateDecoder.Decode(templatePathOrBase64List[i]);
            if (templ is null || templ.Empty())
                continue;
            if (!MatchTemplateEngine.TryMatch(roiMat, templ, threshold, out double score, out OpenCvSharp.Rect r))
                continue;
            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = i;
                bestR = r;
            }
        }

        if (bestIdx < 0)
            return new FindOneOfTemplatesResult(false, -1, 0, 0, 0, 0, 0);

        return new FindOneOfTemplatesResult(true, bestIdx, bestScore, roi.Left + bestR.X, roi.Top + bestR.Y, bestR.Width, bestR.Height);
    }

    /// <summary>
    /// 每个模板至少在 ROI 内命中一次；返回各模板结果。
    /// </summary>
    /// <param name="roi">屏幕 ROI，含义同 <see cref="FindSingleTemplate"/>。</param>
    /// <param name="templatePathOrBase64List">模板列表，每项为路径或 Base64。</param>
    /// <param name="threshold">相似度阈值，含义同 <see cref="FindSingleTemplate"/>。</param>
    /// <returns><see cref="FindAllTemplatesResult.AllFound"/> 表示是否全部模板均达标；<see cref="FindAllTemplatesResult.Items"/> 为每项的命中情况与屏幕绝对矩形。</returns>
    public static FindAllTemplatesResult FindEachTemplateAtLeastOnce(
        ScreenRoi roi,
        IReadOnlyList<string> templatePathOrBase64List,
        double threshold)
    {
        if (!roi.IsValid)
            return new FindAllTemplatesResult(false, Array.Empty<TemplateMatchItem>());

        using Mat? roiMat = GdiScreenCapture.Capture(roi);
        if (roiMat is null)
            return new FindAllTemplatesResult(false, Array.Empty<TemplateMatchItem>());

        var items = new List<TemplateMatchItem>(templatePathOrBase64List.Count);
        bool all = true;
        for (int i = 0; i < templatePathOrBase64List.Count; i++)
        {
            using Mat? templ = TemplateDecoder.Decode(templatePathOrBase64List[i]);
            if (templ is null || templ.Empty())
            {
                all = false;
                items.Add(new TemplateMatchItem(i, false, 0, 0, 0, 0, 0));
                continue;
            }

            if (!MatchTemplateEngine.TryMatch(roiMat, templ, threshold, out double score, out OpenCvSharp.Rect r))
            {
                all = false;
                items.Add(new TemplateMatchItem(i, false, 0, 0, 0, 0, 0));
                continue;
            }

            items.Add(new TemplateMatchItem(i, true, score, roi.Left + r.X, roi.Top + r.Y, r.Width, r.Height));
        }

        return new FindAllTemplatesResult(all, items);
    }

    /// <summary>
    /// ROI 内查找与指定 RGB **完全一致** 的像素（无容差参数），返回 **屏幕绝对坐标**。
    /// </summary>
    /// <param name="roi">屏幕 ROI；先经 Win32 截屏再比较。</param>
    /// <param name="rgb">目标颜色，<b>R、G、B</b> 顺序（与 CSS / 常见 UI 一致）；内部转为 OpenCV BGR。</param>
    /// <returns>颜色完全匹配的像素之屏幕坐标列表；无效 ROI 或截屏失败时返回空只读列表。</returns>
    public static IReadOnlyList<ScreenPoint> FindPixelsRgb(ScreenRoi roi, RgbColor rgb)
    {
        if (!roi.IsValid)
            return Array.Empty<ScreenPoint>();

        using Mat? roiMat = GdiScreenCapture.Capture(roi);
        if (roiMat is null)
            return Array.Empty<ScreenPoint>();

        Scalar bgr = RgbToBgrScalar(rgb);
        using var mask = new Mat();
        Cv2.InRange(roiMat, bgr, bgr, mask);
        return CollectNonZeroScreenPoints(roi, mask);
    }

    /// <summary>
    /// ROI 内查找与 **任意一个** 指定 RGB **完全一致** 的像素（无容差），返回 **屏幕绝对坐标** 列表。
    /// </summary>
    /// <param name="roi">屏幕 ROI；仅截屏一次，再对多种颜色合并掩膜。</param>
    /// <param name="rgbColors">目标 RGB 列表；重复颜色会重复计算但不影响结果。</param>
    /// <returns>匹配任一颜色的像素之屏幕坐标列表；无效 ROI、空列表或截屏失败时返回空只读列表。</returns>
    public static IReadOnlyList<ScreenPoint> FindPixelsRgbMultiple(ScreenRoi roi, IReadOnlyList<RgbColor> rgbColors)
    {
        if (!roi.IsValid || rgbColors.Count == 0)
            return Array.Empty<ScreenPoint>();

        using Mat? roiMat = GdiScreenCapture.Capture(roi);
        if (roiMat is null)
            return Array.Empty<ScreenPoint>();

        using var combined = new Mat(roiMat.Rows, roiMat.Cols, MatType.CV_8UC1, Scalar.All(0));
        foreach (RgbColor rgb in rgbColors)
        {
            Scalar bgr = RgbToBgrScalar(rgb);
            using var mask = new Mat();
            Cv2.InRange(roiMat, bgr, bgr, mask);
            Cv2.BitwiseOr(combined, mask, combined);
        }

        return CollectNonZeroScreenPoints(roi, combined);
    }

    /// <summary>
    /// ROI 内将截屏转为 HSV 后，查找与指定 HSV **完全一致** 的像素（无容差参数）。
    /// </summary>
    /// <param name="roi">屏幕 ROI。</param>
    /// <param name="hsv">目标 HSV（OpenCV 8 位惯例：H∈[0,180]，S、V∈[0,255]）。</param>
    /// <returns>完全匹配的像素之屏幕坐标列表；无效 ROI 或截屏失败时返回空只读列表。</returns>
    public static IReadOnlyList<ScreenPoint> FindPixelsHsv(ScreenRoi roi, Scalar hsv)
    {
        if (!roi.IsValid)
            return Array.Empty<ScreenPoint>();

        using Mat? roiMat = GdiScreenCapture.Capture(roi);
        if (roiMat is null)
            return Array.Empty<ScreenPoint>();

        using var hsvMat = new Mat();
        Cv2.CvtColor(roiMat, hsvMat, ColorConversionCodes.BGR2HSV);
        using var mask = new Mat();
        Cv2.InRange(hsvMat, hsv, hsv, mask);
        return CollectNonZeroScreenPoints(roi, mask);
    }

    /// <summary>
    /// ROI 内与指定 RGB **完全一致** 的像素个数（无容差参数）。
    /// </summary>
    /// <param name="roi">屏幕 ROI。</param>
    /// <param name="rgb">目标 RGB 颜色。</param>
    /// <returns>完全匹配的像素数；无效 ROI 或截屏失败时为 0。</returns>
    public static int CountPixelsRgb(ScreenRoi roi, RgbColor rgb)
    {
        if (!roi.IsValid)
            return 0;

        using Mat? roiMat = GdiScreenCapture.Capture(roi);
        if (roiMat is null)
            return 0;

        Scalar bgr = RgbToBgrScalar(rgb);
        using var mask = new Mat();
        Cv2.InRange(roiMat, bgr, bgr, mask);
        return (int)Cv2.CountNonZero(mask);
    }

    /// <summary>
    /// 屏幕绝对坐标处像素 RGB（经 1×1 Win32 截屏；内部由 BGR 转回 RGB）。
    /// </summary>
    /// <param name="screenX">屏幕/虚拟桌面 X（逻辑像素）。</param>
    /// <param name="screenY">屏幕/虚拟桌面 Y（逻辑像素）。</param>
    /// <param name="rgb">成功时写入 R、G、B；失败时为默认值。</param>
    /// <returns>截屏并读点成功为 <c>true</c>；坐标越界或 GDI 失败为 <c>false</c>。</returns>
    public static bool TryGetScreenPixelRgb(int screenX, int screenY, out RgbColor rgb)
    {
        rgb = default;
        var one = new ScreenRoi(screenX, screenY, 1, 1);
        using Mat? m = GdiScreenCapture.Capture(one);
        if (m is null || m.Empty())
            return false;
        Vec3b bgr = m.Get<Vec3b>(0, 0);
        rgb = BgrVecToRgb(bgr);
        return true;
    }

    /// <summary>对外 RGB → OpenCV BGR <see cref="Scalar"/>。</summary>
    private static Scalar RgbToBgrScalar(RgbColor c) => new Scalar(c.B, c.G, c.R);

    /// <summary>OpenCV <see cref="Vec3b"/>（B,G,R）→ 对外 <see cref="RgbColor"/>。</summary>
    private static RgbColor BgrVecToRgb(Vec3b bgr) => new RgbColor(bgr.Item2, bgr.Item1, bgr.Item0);

    /// <summary>
    /// 截取虚拟桌面为 BGR <see cref="Mat"/>（调用方负责 <c>Dispose</c>）；失败时返回 <c>null</c>。
    /// </summary>
    public static Mat? CaptureVirtualScreenToMat() => GdiScreenCapture.CaptureVirtualScreen();

    /// <summary>
    /// 按屏幕 ROI 截取 BGR <see cref="Mat"/>（调用方负责 <c>Dispose</c>）；ROI 无效或越界时返回 <c>null</c>。
    /// </summary>
    public static Mat? CaptureRegionToMat(ScreenRoi roi) => GdiScreenCapture.Capture(roi);

    /// <summary>
    /// 虚拟桌面全屏截图，输出文件路径或 Base64。
    /// </summary>
    /// <param name="options">输出形式（文件路径 / Base64）、编码格式、JPEG 质量、Data URL 等，见 <see cref="ScreenshotCaptureOptions"/>。</param>
    /// <returns>文件输出时为保存路径字符串；Base64 输出时为纯 Base64 或带前缀的 Data URL。</returns>
    /// <exception cref="InvalidOperationException">虚拟桌面截屏失败（如 GDI 错误）。</exception>
    /// <exception cref="ArgumentException"><paramref name="options"/> 中必填项不合法（如 FilePath 模式未给路径）。</exception>
    public static string CaptureFullScreen(ScreenshotCaptureOptions options)
    {
        using Mat? mat = GdiScreenCapture.CaptureVirtualScreen();
        if (mat is null || mat.Empty())
            throw new InvalidOperationException("全屏截屏失败。");
        return ScreenshotEncoding.EncodeAndDeliver(mat, options);
    }

    /// <summary>
    /// 指定 ROI 截图，输出文件路径或 Base64。
    /// </summary>
    /// <param name="roi">屏幕 ROI，须完全在虚拟桌面内。</param>
    /// <param name="options">输出与编码选项，同 <see cref="CaptureFullScreen"/>。</param>
    /// <returns>文件路径或 Base64 字符串，规则同 <see cref="CaptureFullScreen"/>。</returns>
    /// <exception cref="ArgumentException"><paramref name="roi"/> 无效（宽高非正）。</exception>
    /// <exception cref="InvalidOperationException">区域截屏失败（越界或 GDI/编码错误）。</exception>
    public static string CaptureRegion(ScreenRoi roi, ScreenshotCaptureOptions options)
    {
        if (!roi.IsValid)
            throw new ArgumentException("ROI 无效。", nameof(roi));

        using Mat? mat = GdiScreenCapture.Capture(roi);
        if (mat is null || mat.Empty())
            throw new InvalidOperationException("区域截屏失败（越界或 GDI 错误）。");
        return ScreenshotEncoding.EncodeAndDeliver(mat, options);
    }

    /// <summary>
    /// 将掩膜中非零像素转为屏幕绝对坐标。
    /// </summary>
    /// <param name="roi">用于将掩膜坐标平移为屏幕绝对坐标。</param>
    /// <param name="mask">单通道二值掩膜，与 ROI 图像同尺寸。</param>
    /// <returns>非零像素对应的屏幕坐标列表。</returns>
    private static List<ScreenPoint> CollectNonZeroScreenPoints(ScreenRoi roi, Mat mask)
    {
        using var nz = new Mat();
        Cv2.FindNonZero(mask, nz);
        var list = new List<ScreenPoint>(nz.Rows);
        for (int i = 0; i < nz.Rows; i++)
        {
            var p = nz.Get<Vec2i>(i, 0);
            list.Add(new ScreenPoint(roi.Left + p.Item0, roi.Top + p.Item1));
        }

        return list;
    }
}
