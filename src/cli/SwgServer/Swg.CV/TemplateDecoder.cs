using OpenCvSharp;

namespace Swg.CV;

/// <summary>
/// 模板/图像解码：支持本地文件路径、<c>data:image/...;base64,</c> Data URL、裸 Base64 字符串或原始图像字节（与找图入口一致）。
/// </summary>
public static class TemplateDecoder
{
    /// <summary>
    /// 自文件路径、Data URL 或裸 Base64 图像字节解码为 BGR <see cref="Mat"/>（调用方负责 <c>Dispose</c>）。
    /// </summary>
    /// <param name="pathOrBase64">存在的文件路径，或以 <c>data:image/</c> 开头的 Data URL，或为图像文件的 Base64 正文。</param>
    /// <returns>解码成功且 OpenCV 能识别格式时返回非空 <see cref="Mat"/>；否则 <c>null</c>。</returns>
    public static Mat? Decode(string pathOrBase64)
    {
        if (string.IsNullOrWhiteSpace(pathOrBase64))
            return null;

        string s = pathOrBase64.Trim();
        if (File.Exists(s))
            return Cv2.ImRead(s, ImreadModes.Color);

        if (s.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            byte[]? bytes = ExtractBase64FromDataUrl(s);
            if (bytes is null)
                return null;
            return Cv2.ImDecode(bytes, ImreadModes.Color);
        }

        try
        {
            byte[] bytes = Convert.FromBase64String(s);
            return Cv2.ImDecode(bytes, ImreadModes.Color);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    /// <summary>
    /// 自已编码的图像字节（如 PNG/JPEG 文件内容）解码为 BGR <see cref="Mat"/>（调用方负责 <c>Dispose</c>）。
    /// </summary>
    /// <param name="imageFileBytes">图像文件的完整字节（非 Base64 文本的 UTF-8 字节）。</param>
    /// <returns>与 <see cref="Decode(string)"/> 中 <c>ImDecode</c> 分支相同语义。</returns>
    public static Mat? DecodeFromBytes(byte[] imageFileBytes)
    {
        ArgumentNullException.ThrowIfNull(imageFileBytes);
        if (imageFileBytes.Length == 0)
            return null;
        return Cv2.ImDecode(imageFileBytes, ImreadModes.Color);
    }

    private static byte[]? ExtractBase64FromDataUrl(string dataUrl)
    {
        int i = dataUrl.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
        if (i < 0)
            return null;
        try
        {
            return Convert.FromBase64String(dataUrl[(i + "base64,".Length)..]);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
