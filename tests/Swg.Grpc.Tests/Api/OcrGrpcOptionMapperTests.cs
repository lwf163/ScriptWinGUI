using Swg.OCR;
using Swg.Grpc.Api;
using Xunit;

namespace Swg.Grpc.Tests.Api;

public class OcrGrpcOptionMapperTests
{
    [Fact]
    public void ToOptions_Defaults_ReturnsPaddleSharpChinese()
    {
        var opt = OcrGrpcOptionMapper.ToOptions(null, null, null);
        Assert.Equal(OcrEngineKind.PaddleSharp, opt.Engine);
        Assert.Equal(OcrLanguage.Chinese, opt.Language);
        Assert.Equal(PaddleChineseModelVersion.V3, opt.PaddleChineseModel);
    }

    [Fact]
    public void ToOptions_TesseractEnglish_Works()
    {
        var opt = OcrGrpcOptionMapper.ToOptions("Tesseract", "English", null);
        Assert.Equal(OcrEngineKind.Tesseract, opt.Engine);
        Assert.Equal(OcrLanguage.English, opt.Language);
    }

    [Fact]
    public void ToOptions_ChineseV4_Works()
    {
        var opt = OcrGrpcOptionMapper.ToOptions(null, null, "ChineseV4");
        Assert.Equal(PaddleChineseModelVersion.V4, opt.PaddleChineseModel);
    }

    [Fact]
    public void ToOptions_V5_Works()
    {
        var opt = OcrGrpcOptionMapper.ToOptions(null, null, "V5");
        Assert.Equal(PaddleChineseModelVersion.V5, opt.PaddleChineseModel);
    }

    [Fact]
    public void ToOptions_V3_Works()
    {
        var opt = OcrGrpcOptionMapper.ToOptions(null, null, "V3");
        Assert.Equal(PaddleChineseModelVersion.V3, opt.PaddleChineseModel);
    }

    [Fact]
    public void ToOptions_InvalidEngine_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            OcrGrpcOptionMapper.ToOptions("BadEngine", null, null));
        Assert.Contains("无效的 Engine", ex.Message);
    }

    [Fact]
    public void ToOptions_InvalidLanguage_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            OcrGrpcOptionMapper.ToOptions(null, "Klingon", null));
        Assert.Contains("无效的 Language", ex.Message);
    }

    [Fact]
    public void ToOptions_InvalidPaddleModel_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            OcrGrpcOptionMapper.ToOptions(null, null, "V99"));
        Assert.Contains("无效的 PaddleChineseModel", ex.Message);
    }

    [Fact]
    public void ToOptions_TableWithTesseract_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            OcrGrpcOptionMapper.ToOptions("Tesseract", "Chinese", null, table: true));
        Assert.Contains("表格识别仅支持 Engine=PaddleSharp", ex.Message);
    }

    [Theory]
    [InlineData(null, OcrLanguage.Chinese)]
    [InlineData("", OcrLanguage.Chinese)]
    [InlineData("Chinese", OcrLanguage.Chinese)]
    [InlineData("chinese", OcrLanguage.Chinese)]
    [InlineData("English", OcrLanguage.English)]
    [InlineData("english", OcrLanguage.English)]
    public void ParseLanguage_Works(string? input, OcrLanguage expected)
    {
        Assert.Equal(expected, OcrGrpcOptionMapper.ParseLanguage(input));
    }
}
