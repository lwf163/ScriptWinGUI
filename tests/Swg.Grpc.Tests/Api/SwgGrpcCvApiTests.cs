using Swg.Grpc.Api;
using Swg.Grpc.Cv;
using Xunit;

namespace Swg.Grpc.Tests.Api;

public class SwgGrpcCvApiTests
{
    [Fact]
    public void FindSingleTemplate_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcCvApi.FindSingleTemplate(null!));
    }

    [Fact]
    public void FindSingleTemplate_EmptyTemplate_ThrowsArgumentException()
    {
        var request = new FindSingleTemplateRequest
        {
            Template = "",
            Roi = new Roi { Left = 0, Top = 0, Width = 100, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcCvApi.FindSingleTemplate(request));
        Assert.Contains("Template", ex.Message);
    }

    [Fact]
    public void FindSingleTemplate_NullRoi_ThrowsArgumentException()
    {
        var request = new FindSingleTemplateRequest { Template = "test.png" };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcCvApi.FindSingleTemplate(request));
        Assert.Contains("Roi", ex.Message);
    }

    [Fact]
    public void FindSingleTemplate_ZeroSizeRoi_ThrowsArgumentException()
    {
        var request = new FindSingleTemplateRequest
        {
            Template = "test.png",
            Roi = new Roi { Left = 0, Top = 0, Width = 0, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcCvApi.FindSingleTemplate(request));
        Assert.Contains("Width/Height", ex.Message);
    }

    [Fact]
    public void FindOneOfTemplates_EmptyTemplates_ThrowsArgumentException()
    {
        var request = new FindOneOfTemplatesRequest
        {
            Roi = new Roi { Left = 0, Top = 0, Width = 100, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcCvApi.FindOneOfTemplates(request));
        Assert.Contains("Templates", ex.Message);
    }

    [Fact]
    public void FindPixelsRgb_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcCvApi.FindPixelsRgb(null!));
    }

    [Fact]
    public void FindPixelsRgb_NullRgb_ThrowsArgumentException()
    {
        var request = new FindPixelsRgbRequest
        {
            Roi = new Roi { Left = 0, Top = 0, Width = 100, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcCvApi.FindPixelsRgb(request));
        Assert.Contains("Rgb", ex.Message);
    }

    [Fact]
    public void FindPixelsRgbMultiple_EmptyColors_ThrowsArgumentException()
    {
        var request = new FindPixelsRgbMultipleRequest
        {
            Roi = new Roi { Left = 0, Top = 0, Width = 100, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcCvApi.FindPixelsRgbMultiple(request));
        Assert.Contains("RgbColors", ex.Message);
    }

    [Fact]
    public void FindPixelsHsv_NullHsv_ThrowsArgumentException()
    {
        var request = new FindPixelsHsvRequest
        {
            Roi = new Roi { Left = 0, Top = 0, Width = 100, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcCvApi.FindPixelsHsv(request));
        Assert.Contains("Hsv", ex.Message);
    }

    [Fact]
    public void CountPixelsRgb_NullRgb_ThrowsArgumentException()
    {
        var request = new CountPixelsRgbRequest
        {
            Roi = new Roi { Left = 0, Top = 0, Width = 100, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcCvApi.CountPixelsRgb(request));
        Assert.Contains("Rgb", ex.Message);
    }

    [Fact]
    public void GetPixelRgb_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcCvApi.GetPixelRgb(null!));
    }

    [Fact]
    public void CheckWindowRoiConsistency_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcCvApi.CheckWindowRoiConsistency(null!));
    }

    [Fact]
    public void CheckWindowRoiConsistency_EmptyHandle_ThrowsArgumentException()
    {
        var request = new WindowRoiConsistencyRequest { WindowHandle = "" };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcCvApi.CheckWindowRoiConsistency(request));
        Assert.Contains("WindowHandle", ex.Message);
    }

    [Fact]
    public void CaptureFullScreen_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcCvApi.CaptureFullScreen(null!));
    }

    [Fact]
    public void CaptureRegion_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcCvApi.CaptureRegion(null!));
    }
}
