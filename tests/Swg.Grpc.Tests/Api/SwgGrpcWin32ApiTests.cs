using Swg.Grpc.Api;
using Swg.Grpc.Win32;
using Xunit;

namespace Swg.Grpc.Tests.Api;

public class SwgGrpcWin32ApiTests
{
    [Fact]
    public void FindWindow_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.FindWindow(null!));
    }

    [Fact]
    public void FindWindow_BothFiltersEmpty_ThrowsArgumentException()
    {
        var request = new FindWindowRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcWin32Api.FindWindow(request));
        Assert.Contains("TitleContains", ex.Message);
    }

    [Fact]
    public void SetForegroundWindow_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.SetForegroundWindow(null!));
    }

    [Fact]
    public void GetWindowInfo_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.GetWindowInfo(null!));
    }

    [Fact]
    public void SetWindowPositionResize_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.SetWindowPositionResize(null!));
    }

    [Fact]
    public void SetWindowState_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.SetWindowState(null!));
    }

    [Fact]
    public void CloseWindow_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.CloseWindow(null!));
    }

    [Fact]
    public void GetWindowProcessId_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.GetWindowProcessId(null!));
    }

    [Fact]
    public void EnumChildWindows_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.EnumChildWindows(null!));
    }

    [Fact]
    public void FindChildWindow_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.FindChildWindow(null!));
    }

    [Fact]
    public void StartProcess_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.StartProcess(null!));
    }

    [Fact]
    public void KillProcess_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.KillProcess(null!));
    }

    [Fact]
    public void ProcessExists_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.ProcessExists(null!));
    }

    [Fact]
    public void ProcessWaitExit_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.ProcessWaitExit(null!));
    }

    [Fact]
    public void SetClipboardText_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.SetClipboardText(null!));
    }

    [Fact]
    public void SendMessage_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.SendMessage(null!));
    }

    [Fact]
    public void PostMessage_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.PostMessage(null!));
    }

    [Fact]
    public void WindowFromPoint_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.WindowFromPoint(null!));
    }

    [Fact]
    public void SendKeys_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.SendKeys(null!));
    }

    [Fact]
    public void GetControlText_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.GetControlText(null!));
    }

    [Fact]
    public void SendWmCommand_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcWin32Api.SendWmCommand(null!));
    }
}
