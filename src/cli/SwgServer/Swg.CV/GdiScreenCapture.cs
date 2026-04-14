using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenCvSharp;

namespace Swg.CV;

/// <summary>
/// Win32 GDI 截屏：BitBlt → 位图 → BGR <see cref="Mat"/>。
/// </summary>
public static class GdiScreenCapture
{
    /// <summary>与需求文档一致：面积 ≤ 此值采用策略 A。</summary>
    public const int StrategyAreaThreshold = 1920 * 1080;

    /// <summary>截取虚拟桌面矩形（全屏 §7）。</summary>
    public static Mat? CaptureVirtualScreen()
    {
        int vx = Win32Native.GetSystemMetrics(Win32Native.SmXVirtualScreen);
        int vy = Win32Native.GetSystemMetrics(Win32Native.SmYVirtualScreen);
        int vw = Win32Native.GetSystemMetrics(Win32Native.SmCxVirtualScreen);
        int vh = Win32Native.GetSystemMetrics(Win32Native.SmCyVirtualScreen);
        if (vw <= 0 || vh <= 0)
            return null;

        return CaptureCore(vx, vy, vw, vh);
    }

    /// <summary>按屏幕 ROI 截取；矩形须完全落在虚拟桌面内。</summary>
    public static Mat? Capture(ScreenRoi roi)
    {
        if (!roi.IsValid)
            return null;

        if (!IsRoiFullyInsideVirtualScreen(roi))
            return null;

        return CaptureCore(roi.Left, roi.Top, roi.Width, roi.Height);
    }

    public static float GetSystemDpiScale()
    {
        nint hdc = Win32Native.GetDC(0);
        if (hdc == 0)
            return 1f;
        try
        {
            int dpi = Win32Native.GetDeviceCaps(hdc, Win32Native.LogPixelsX);
            return dpi <= 0 ? 1f : dpi / 96f;
        }
        finally
        {
            Win32Native.ReleaseDC(0, hdc);
        }
    }

    /// <summary>判断 ROI 是否完全落在当前虚拟桌面矩形内（截屏前可调用以避免无效请求）。</summary>
    public static bool IsRoiFullyInsideVirtualScreen(ScreenRoi roi)
    {
        int vx = Win32Native.GetSystemMetrics(Win32Native.SmXVirtualScreen);
        int vy = Win32Native.GetSystemMetrics(Win32Native.SmYVirtualScreen);
        int vw = Win32Native.GetSystemMetrics(Win32Native.SmCxVirtualScreen);
        int vh = Win32Native.GetSystemMetrics(Win32Native.SmCyVirtualScreen);
        long r = (long)roi.Left + roi.Width;
        long b = (long)roi.Top + roi.Height;
        return roi.Left >= vx && roi.Top >= vy && r <= (long)vx + vw && b <= (long)vy + vh;
    }

    private static Mat? CaptureCore(int srcX, int srcY, int width, int height)
    {
        nint hdcScreen = Win32Native.GetDC(0);
        if (hdcScreen == 0)
            return null;

        nint hdcMem = 0;
        nint hBmp = 0;
        nint oldObj = 0;
        try
        {
            hdcMem = Win32Native.CreateCompatibleDC(hdcScreen);
            if (hdcMem == 0)
                return null;

            hBmp = Win32Native.CreateCompatibleBitmap(hdcScreen, width, height);
            if (hBmp == 0)
                return null;

            oldObj = Win32Native.SelectObject(hdcMem, hBmp);
            if (!Win32Native.BitBlt(hdcMem, 0, 0, width, height, hdcScreen, srcX, srcY, Win32Native.Srccopy))
                return null;

            Win32Native.SelectObject(hdcMem, oldObj);
            oldObj = 0;

            using var bmp = Image.FromHbitmap(hBmp);
            return BitmapToBgrMat(bmp);
        }
        finally
        {
            if (oldObj != 0 && hdcMem != 0)
                Win32Native.SelectObject(hdcMem, oldObj);
            if (hBmp != 0)
                Win32Native.DeleteObject(hBmp);
            if (hdcMem != 0)
                Win32Native.DeleteDC(hdcMem);
            Win32Native.ReleaseDC(0, hdcScreen);
        }
    }

    private static Mat BitmapToBgrMat(Bitmap bmp)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int stride = Math.Abs(data.Stride);
            var packed = new byte[w * h * 4];
            for (int y = 0; y < h; y++)
            {
                nint rowPtr = data.Scan0 + y * data.Stride;
                Marshal.Copy(rowPtr, packed, y * w * 4, w * 4);
            }

            using var bgra = Mat.FromPixelData(h, w, MatType.CV_8UC4, packed);
            var bgr = new Mat();
            Cv2.CvtColor(bgra, bgr, ColorConversionCodes.BGRA2BGR);
            return bgr;
        }
        finally
        {
            bmp.UnlockBits(data);
        }
    }
}
