using OpenCvSharp;

namespace Swg.CV;

/// <summary>
/// 将 BGR <see cref="Mat"/> 按 <see cref="ScreenshotCaptureOptions"/> 编码并输出为文件路径或 Base64。
/// </summary>
public static class ScreenshotEncoding
{
    public static string EncodeAndDeliver(Mat bgr, ScreenshotCaptureOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        string ext = options.ImageFormat == ScreenshotImageFormat.Png ? ".png" : ".jpg";
        byte[] buf;
        if (options.ImageFormat == ScreenshotImageFormat.Jpeg)
        {
            var prm = new ImageEncodingParam(ImwriteFlags.JpegQuality, Math.Clamp(options.JpegQuality, 0, 100));
            Cv2.ImEncode(ext, bgr, out buf, prm);
        }
        else
        {
            Cv2.ImEncode(ext, bgr, out buf);
        }

        if (options.OutputKind == ScreenshotOutputKind.FilePath)
        {
            if (string.IsNullOrWhiteSpace(options.TargetFilePath))
                throw new ArgumentException("TargetFilePath 在 FilePath 输出形式下必填。", nameof(options));
            string dir = Path.GetDirectoryName(options.TargetFilePath)!;
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(options.TargetFilePath, buf);
            return options.TargetFilePath;
        }

        string b64 = Convert.ToBase64String(buf);
        if (options.Base64Variant == ScreenshotBase64Variant.DataUrl)
        {
            string mime = options.ImageFormat == ScreenshotImageFormat.Png ? "image/png" : "image/jpeg";
            return $"data:{mime};base64,{b64}";
        }

        return b64;
    }
}
