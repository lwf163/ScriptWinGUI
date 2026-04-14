using Swg.Grpc.Api;
using Swg.Grpc.Ocr;
using Xunit;

namespace Swg.Grpc.Tests.Api;

public class SwgGrpcOcrApiTests
{
    [Fact]
    public void RecognizeScreenStrings_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcOcrApi.RecognizeScreenStrings(null!));
    }

    [Fact]
    public void RecognizeScreenStrings_NullRoi_ThrowsArgumentException()
    {
        var request = new OcrScreenStringsRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcOcrApi.RecognizeScreenStrings(request));
        Assert.Contains("Roi", ex.Message);
    }

    [Fact]
    public void RecognizeScreenStrings_ZeroSizeRoi_ThrowsArgumentException()
    {
        var request = new OcrScreenStringsRequest
        {
            Roi = new Roi { Left = 0, Top = 0, Width = 0, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcOcrApi.RecognizeScreenStrings(request));
        Assert.Contains("Width/Height", ex.Message);
    }

    [Fact]
    public void RecognizeScreenStringsMatch_NullMatchText_ThrowsArgumentException()
    {
        var request = new OcrScreenMatchRequest
        {
            Roi = new Roi { Left = 0, Top = 0, Width = 100, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcOcrApi.RecognizeScreenStringsMatch(request));
        Assert.Contains("MatchText", ex.Message);
    }

    [Fact]
    public void RecognizeScreenStringsMatch_EmptyMatchText_ThrowsArgumentException()
    {
        var request = new OcrScreenMatchRequest
        {
            MatchText = "",
            Roi = new Roi { Left = 0, Top = 0, Width = 100, Height = 100 },
        };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcOcrApi.RecognizeScreenStringsMatch(request));
        Assert.Contains("MatchText", ex.Message);
    }

    [Fact]
    public void RecognizeScreenTable_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcOcrApi.RecognizeScreenTable(null!));
    }

    [Fact]
    public void RecognizeImageStrings_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcOcrApi.RecognizeImageStrings(null!));
    }

    [Fact]
    public void RecognizeImageStrings_EmptyImage_ThrowsArgumentException()
    {
        var request = new OcrImageStringsRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcOcrApi.RecognizeImageStrings(request));
        Assert.Contains("Image", ex.Message);
    }

    [Fact]
    public void RecognizeImageTable_EmptyImage_ThrowsArgumentException()
    {
        var request = new OcrImageTableRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcOcrApi.RecognizeImageTable(request));
        Assert.Contains("Image", ex.Message);
    }

    [Fact]
    public void RecognizeScreenQuickTable_NullRoi_ThrowsArgumentException()
    {
        var request = new OcrScreenQuickTableRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcOcrApi.RecognizeScreenQuickTable(request));
        Assert.Contains("Roi", ex.Message);
    }

    [Fact]
    public void RecognizeImageQuickTable_EmptyImage_ThrowsArgumentException()
    {
        var request = new OcrImageQuickTableRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcOcrApi.RecognizeImageQuickTable(request));
        Assert.Contains("Image", ex.Message);
    }
}
