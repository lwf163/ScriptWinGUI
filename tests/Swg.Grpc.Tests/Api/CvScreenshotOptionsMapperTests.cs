using Swg.CV;
using Swg.Grpc.Api;
using Swg.Grpc.Cv;
using Xunit;

namespace Swg.Grpc.Tests.Api;

public class CvScreenshotOptionsMapperTests
{
    [Fact]
    public void ToOptions_NullDto_ReturnsDefaults()
    {
        var opt = CvGrpcScreenshotOptionsMapper.ToOptions(null);
        Assert.Equal(ScreenshotOutputKind.Base64, opt.OutputKind);
        Assert.Equal(ScreenshotImageFormat.Png, opt.ImageFormat);
        Assert.Equal(ScreenshotBase64Variant.Raw, opt.Base64Variant);
        Assert.Equal(0, opt.JpegQuality);
    }

    [Fact]
    public void ToOptions_FilePathWithoutPath_ThrowsArgumentException()
    {
        var dto = new ScreenshotOptions { OutputKind = "FilePath" };
        var ex = Assert.Throws<ArgumentException>(() =>
            CvGrpcScreenshotOptionsMapper.ToOptions(dto));
        Assert.Contains("TargetFilePath 必填", ex.Message);
    }

    [Fact]
    public void ToOptions_FilePathWithPath_Works()
    {
        var dto = new ScreenshotOptions { OutputKind = "FilePath", TargetFilePath = "C:\\out.png" };
        var opt = CvGrpcScreenshotOptionsMapper.ToOptions(dto);
        Assert.Equal(ScreenshotOutputKind.FilePath, opt.OutputKind);
        Assert.Equal("C:\\out.png", opt.TargetFilePath);
    }

    [Fact]
    public void ToOptions_JpegWithQuality_Works()
    {
        var dto = new ScreenshotOptions
        {
            OutputKind = "Base64",
            ImageFormat = "Jpeg",
            JpegQuality = 75,
        };
        var opt = CvGrpcScreenshotOptionsMapper.ToOptions(dto);
        Assert.Equal(ScreenshotImageFormat.Jpeg, opt.ImageFormat);
        Assert.Equal(75, opt.JpegQuality);
    }

    [Fact]
    public void ToOptions_DataUrlVariant_Works()
    {
        var dto = new ScreenshotOptions { Base64Variant = "DataUrl" };
        var opt = CvGrpcScreenshotOptionsMapper.ToOptions(dto);
        Assert.Equal(ScreenshotBase64Variant.DataUrl, opt.Base64Variant);
    }

    [Fact]
    public void ToOptions_InvalidOutputKind_ThrowsArgumentException()
    {
        var dto = new ScreenshotOptions { OutputKind = "BadKind" };
        var ex = Assert.Throws<ArgumentException>(() =>
            CvGrpcScreenshotOptionsMapper.ToOptions(dto));
        Assert.Contains("无效的 OutputKind", ex.Message);
    }

    [Fact]
    public void ToOptions_QualityClamped_Over100()
    {
        var dto = new ScreenshotOptions { JpegQuality = 200 };
        var opt = CvGrpcScreenshotOptionsMapper.ToOptions(dto);
        Assert.Equal(100, opt.JpegQuality);
    }

    [Fact]
    public void ToOptions_QualityClamped_Negative()
    {
        var dto = new ScreenshotOptions { JpegQuality = -10 };
        var opt = CvGrpcScreenshotOptionsMapper.ToOptions(dto);
        Assert.Equal(0, opt.JpegQuality);
    }

    [Fact]
    public void ToOptions_Base64Raw_Works()
    {
        var dto = new ScreenshotOptions { Base64Variant = "Raw" };
        var opt = CvGrpcScreenshotOptionsMapper.ToOptions(dto);
        Assert.Equal(ScreenshotBase64Variant.Raw, opt.Base64Variant);
    }
}
