using Swg.Grpc.Api;
using Swg.Grpc.Flaui;
using Xunit;

namespace Swg.Grpc.Tests.Api;

public class SwgGrpcFlaUiApiTests
{
    [Fact]
    public void CreateSession_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.CreateSession(null!));
    }

    [Fact]
    public void DeleteSession_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.DeleteSession(null!));
    }

    [Fact]
    public void CloseApplication_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.CloseApplication(null!));
    }

    [Fact]
    public void KillApplication_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.KillApplication(null!));
    }

    [Fact]
    public void WaitWhileBusy_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.WaitWhileBusy(null!));
    }

    [Fact]
    public void WaitMainHandle_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.WaitMainHandle(null!));
    }

    [Fact]
    public void GetMainWindow_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.GetMainWindow(null!));
    }

    [Fact]
    public void GetTopLevelWindows_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.GetTopLevelWindows(null!));
    }

    [Fact]
    public void GetElementInfo_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.GetElementInfo(null!));
    }

    [Fact]
    public void FindElement_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.FindElement(null!));
    }

    [Fact]
    public void FindElement_NullFind_ThrowsArgumentNullException()
    {
        var request = new SessionFindElementRequest();
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.FindElement(request));
    }

    [Fact]
    public void FindAllElements_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.FindAllElements(null!));
    }

    [Fact]
    public void FindElementByXPath_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.FindElementByXPath(null!));
    }

    [Fact]
    public void FindElementByXPath_NullFind_ThrowsArgumentNullException()
    {
        var request = new SessionFindByXPathRequest();
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.FindElementByXPath(request));
    }

    [Fact]
    public void FindAllElementsByXPath_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.FindAllElementsByXPath(null!));
    }

    [Fact]
    public void GetChildren_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.GetChildren(null!));
    }

    [Fact]
    public void Focus_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.Focus(null!));
    }

    [Fact]
    public void FocusNative_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.FocusNative(null!));
    }

    [Fact]
    public void SetElementForeground_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.SetElementForeground(null!));
    }

    [Fact]
    public void Click_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.Click(null!));
    }

    [Fact]
    public void DoubleClick_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.DoubleClick(null!));
    }

    [Fact]
    public void RightClick_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.RightClick(null!));
    }

    [Fact]
    public void RightDoubleClick_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFlaUiApi.RightDoubleClick(null!));
    }
}
