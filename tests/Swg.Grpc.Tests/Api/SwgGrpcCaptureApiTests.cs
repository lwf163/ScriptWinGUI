using Swg.Grpc.Api;
using Swg.Grpc.Capture;
using Xunit;

namespace Swg.Grpc.Tests.Api;

public class SwgGrpcCaptureApiTests
{
    [Fact]
    public void CreateListenWindow_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcCaptureApi.CreateListenWindow(null!));
    }

    [Fact]
    public void StopListenWindow_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcCaptureApi.StopListenWindow(null!));
    }

    [Fact]
    public void StopListenWindow_EmptyId_ThrowsFormatException()
    {
        var request = new CaptureStopListenWindowRequest { ListenWindowId = "" };
        Assert.Throws<FormatException>(() => SwgGrpcCaptureApi.StopListenWindow(request));
    }

    [Fact]
    public void QueryHistory_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcCaptureApi.QueryHistory(null!));
    }

    [Fact]
    public void QueryHistory_InvalidGuid_ThrowsFormatException()
    {
        var request = new CaptureHistoryQueryRequest { ListenWindowId = "not-a-guid" };
        Assert.Throws<FormatException>(() => SwgGrpcCaptureApi.QueryHistory(request));
    }
}
