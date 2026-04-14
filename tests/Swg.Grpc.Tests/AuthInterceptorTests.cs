using System.Security.Cryptography;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;
using SwgServer;
using Xunit;

namespace Swg.Grpc.Tests;

public class AuthInterceptorTests
{
    private static (AuthInterceptor interceptor, string validToken) CreateInterceptor()
    {
        var hmacKey = RandomNumberGenerator.GetBytes(32);
        var secret = Convert.ToBase64String(hmacKey);
        var plain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        using var hmac = new HMACSHA256(hmacKey);
        var signed = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(plain)));
        var validator = new TokenValidator(secret, signed);
        return (new AuthInterceptor(validator), plain);
    }

    [Fact]
    public async Task UnaryServerHandler_ValidToken_PassesThrough()
    {
        var (interceptor, token) = CreateInterceptor();
        var headers = new Metadata { { "authorization", $"Bearer {token}" } };
        var context = new TestServerCallContext(headers);
        var continuation = new UnaryServerMethod<string, string>((req, ctx) => Task.FromResult("ok"));

        var result = await interceptor.UnaryServerHandler("input", context, continuation);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task UnaryServerHandler_InvalidToken_ThrowsUnauthenticated()
    {
        var (interceptor, _) = CreateInterceptor();
        var headers = new Metadata { { "authorization", "Bearer bad-token" } };
        var context = new TestServerCallContext(headers);
        var continuation = new UnaryServerMethod<string, string>((req, ctx) => Task.FromResult("ok"));

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            interceptor.UnaryServerHandler("input", context, continuation));
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task UnaryServerHandler_MissingHeader_ThrowsUnauthenticated()
    {
        var (interceptor, _) = CreateInterceptor();
        var headers = new Metadata();
        var context = new TestServerCallContext(headers);
        var continuation = new UnaryServerMethod<string, string>((req, ctx) => Task.FromResult("ok"));

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            interceptor.UnaryServerHandler("input", context, continuation));
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task UnaryServerHandler_NonBearerScheme_ThrowsUnauthenticated()
    {
        var (interceptor, _) = CreateInterceptor();
        var headers = new Metadata { { "authorization", "Basic abc123" } };
        var context = new TestServerCallContext(headers);
        var continuation = new UnaryServerMethod<string, string>((req, ctx) => Task.FromResult("ok"));

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            interceptor.UnaryServerHandler("input", context, continuation));
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task ServerStreamingServerHandler_ValidToken_PassesThrough()
    {
        var (interceptor, token) = CreateInterceptor();
        var headers = new Metadata { { "authorization", $"Bearer {token}" } };
        var context = new TestServerCallContext(headers);
        var called = false;
        var continuation = new ServerStreamingServerMethod<string, string>((req, stream, ctx) =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await interceptor.ServerStreamingServerHandler("input", null!, context, continuation);
        Assert.True(called);
    }

    [Fact]
    public async Task ServerStreamingServerHandler_InvalidToken_ThrowsUnauthenticated()
    {
        var (interceptor, _) = CreateInterceptor();
        var headers = new Metadata { { "authorization", "Bearer bad" } };
        var context = new TestServerCallContext(headers);
        var continuation = new ServerStreamingServerMethod<string, string>((req, stream, ctx) => Task.CompletedTask);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            interceptor.ServerStreamingServerHandler("input", null!, context, continuation));
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    private sealed class TestServerCallContext : ServerCallContext
    {
        private readonly Metadata _headers;

        public TestServerCallContext(Metadata headers) => _headers = headers;

        protected override string MethodCore => "/test/service/method";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "ipv4:127.0.0.1:12345";
        protected override DateTime DeadlineCore => DateTime.UtcNow.AddHours(1);
        protected override Metadata RequestHeadersCore => _headers;
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore { get; } = [];
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new("", []);
        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) => null!;
        protected override Task WriteResponseHeadersAsyncCore(Metadata headers) => Task.CompletedTask;
    }
}
