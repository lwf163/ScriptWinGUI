using Grpc.Core;
using Grpc.Core.Interceptors;
using Serilog;

internal sealed class AuthInterceptor : Interceptor
{
    private static readonly ILogger Logger = Log.ForContext<AuthInterceptor>();
    private readonly TokenValidator _validator;

    public AuthInterceptor(TokenValidator validator)
    {
        _validator = validator;
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        EnsureAuthenticated(context);
        return continuation(request, context);
    }

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request, IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        EnsureAuthenticated(context);
        return continuation(request, responseStream, context);
    }

    public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        EnsureAuthenticated(context);
        return continuation(requestStream, context);
    }

    public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        EnsureAuthenticated(context);
        return continuation(requestStream, responseStream, context);
    }

    private void EnsureAuthenticated(ServerCallContext context)
    {
        var token = ExtractBearerToken(context.RequestHeaders);
        if (token is null || !_validator.Validate(token))
        {
            Logger.Warning("Authentication failed, remote peer: {Peer}, method: {Method}", context.Peer, context.Method);
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or missing API Token"));
        }
    }

    private static string? ExtractBearerToken(Metadata headers)
    {
        var entry = headers.Get("authorization");
        if (entry is null || !entry.Value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;
        return entry.Value["Bearer ".Length..].Trim();
    }
}
