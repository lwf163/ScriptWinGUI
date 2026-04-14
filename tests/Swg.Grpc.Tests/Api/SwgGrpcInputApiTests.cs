using Swg.Grpc.Api;
using Swg.Grpc.Input;
using Xunit;

namespace Swg.Grpc.Tests.Api;

public class SwgGrpcInputApiTests
{
    [Fact]
    public void TypeText_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.TypeText(null!));
    }

    [Fact]
    public void TypeChar_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.TypeChar(null!));
    }

    [Fact]
    public void TypeKeys_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.TypeKeys(null!));
    }

    [Fact]
    public void TypeSimultaneously_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.TypeSimultaneously(null!));
    }

    [Fact]
    public void TypeKey_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.TypeKey(null!));
    }

    [Fact]
    public void Press_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.Press(null!));
    }

    [Fact]
    public void Release_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.Release(null!));
    }

    [Fact]
    public void TypeSequence_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.TypeSequence(null!));
    }

    [Fact]
    public void SetCursorPosition_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.SetCursorPosition(null!));
    }

    [Fact]
    public void MoveTo_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.MoveTo(null!));
    }

    [Fact]
    public void MoveBy_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.MoveBy(null!));
    }

    [Fact]
    public void SetMoveSettings_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.SetMoveSettings(null!));
    }

    [Fact]
    public void Click_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.Click(null!));
    }

    [Fact]
    public void Down_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.Down(null!));
    }

    [Fact]
    public void Up_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.Up(null!));
    }

    [Fact]
    public void DragTo_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.DragTo(null!));
    }

    [Fact]
    public void DragBy_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.DragBy(null!));
    }

    [Fact]
    public void Scroll_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.Scroll(null!));
    }

    [Fact]
    public void HorizontalScroll_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.HorizontalScroll(null!));
    }

    [Fact]
    public void Wait_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcInputApi.Wait(null!));
    }

    [Fact]
    public void Wait_NegativeMs_ThrowsArgumentException()
    {
        var request = new InputWaitRequest { Milliseconds = -1 };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcInputApi.Wait(request));
        Assert.Contains("Milliseconds", ex.Message);
    }
}
