using OpenCvSharp;

namespace Swg.OCR.QuickTable;

/// <summary>表格线检测前的二值化预处理。</summary>
public sealed class QuickTableImageProcessor
{
    /// <summary>自适应阈值二值化（线条为白）。</summary>
    public Mat PreprocessImage(Mat img, int blurKsize = 3)
    {
        if (img.Empty())
            throw new ArgumentException("图像为空。", nameof(img));

        Mat gray;
        if (img.Channels() == 3)
        {
            gray = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
        }
        else
        {
            gray = img.Clone();
        }

        var bw = new Mat();
        Cv2.AdaptiveThreshold(gray, bw, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 11, 2);
        gray.Dispose();

        return bw;
    }
}
