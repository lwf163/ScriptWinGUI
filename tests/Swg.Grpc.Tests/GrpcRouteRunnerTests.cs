using Grpc.Core;
using Xunit;

namespace Swg.Grpc.Tests;

public class GrpcRouteRunnerTests
{
    [Fact]
    public void Run_Success_ReturnsValue()
    {
        var result = GrpcRouteRunner.Run(() => 42);
        Assert.Equal(42, result);
    }

    [Fact]
    public void Run_ArgumentException_MapsToInvalidArgument()
    {
        var ex = Assert.Throws<RpcException>(() =>
            GrpcRouteRunner.Run<int>(() => throw new ArgumentException("test")));
        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        Assert.Equal("test", ex.Status.Detail);
    }

    [Fact]
    public void Run_InvalidOperationException_MapsToUnavailable()
    {
        var ex = Assert.Throws<RpcException>(() =>
            GrpcRouteRunner.Run<int>(() => throw new InvalidOperationException("ops")));
        Assert.Equal(StatusCode.Unavailable, ex.StatusCode);
    }

    [Fact]
    public void Run_GeneralException_MapsToInternal()
    {
        var ex = Assert.Throws<RpcException>(() =>
            GrpcRouteRunner.Run<int>(() => throw new Exception("boom")));
        Assert.Equal(StatusCode.Internal, ex.StatusCode);
    }

    [Fact]
    public void Run_RpcException_PassesThrough()
    {
        var original = new RpcException(new Status(StatusCode.NotFound, "keep"));
        var ex = Assert.Throws<RpcException>(() =>
            GrpcRouteRunner.Run<int>(() => throw original));
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task RunAsync_T_Success_ReturnsValue()
    {
        var result = await GrpcRouteRunner.RunAsync(() => Task.FromResult("ok"));
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task RunAsync_T_ArgumentException_MapsToInvalidArgument()
    {
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            GrpcRouteRunner.RunAsync<int>(() => throw new ArgumentException("bad")));
        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task RunAsync_T_RpcException_PassesThrough()
    {
        var original = new RpcException(new Status(StatusCode.PermissionDenied, "nope"));
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            GrpcRouteRunner.RunAsync<int>(() => throw original));
        Assert.Equal(StatusCode.PermissionDenied, ex.StatusCode);
    }

    [Fact]
    public async Task RunAsync_Void_Success()
    {
        var called = false;
        await GrpcRouteRunner.RunAsync(() => { called = true; return Task.CompletedTask; });
        Assert.True(called);
    }

    [Fact]
    public async Task RunAsync_Void_InvalidOperationException_MapsToUnavailable()
    {
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            GrpcRouteRunner.RunAsync(() => throw new InvalidOperationException("err")));
        Assert.Equal(StatusCode.Unavailable, ex.StatusCode);
    }

    [Fact]
    public async Task RunAsync_Void_GeneralException_MapsToInternal()
    {
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            GrpcRouteRunner.RunAsync(() => throw new Exception("fatal")));
        Assert.Equal(StatusCode.Internal, ex.StatusCode);
    }
}
