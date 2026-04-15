using System.Collections.Generic;
using System.Globalization;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Serilog;
using Swg.Grpc;

namespace SwgServer;

/// <summary>
/// 合并客户端 deadline、请求头 <c>x-swg-timeout-ms</c> 与服务端默认期限，对 Unary 用 <see cref="Task.WhenAny"/> 约束，
/// 对流式 RPC 推送合并后的 <see cref="CancellationToken"/>（见 <see cref="RpcCallDeadlineContext"/>）。
/// </summary>
internal sealed class RpcDeadlineInterceptor : Interceptor
{
    /// <summary>单次调用可携带的期限（毫秒），自服务端收到请求时起算。</summary>
    public const string TimeoutMetadataKey = "x-swg-timeout-ms";

    private static readonly ILogger Logger = Log.ForContext(typeof(RpcDeadlineInterceptor));

    private readonly int _defaultRpcTimeoutMs;

    public RpcDeadlineInterceptor(int defaultRpcTimeoutMs)
    {
        _defaultRpcTimeoutMs = defaultRpcTimeoutMs;
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var effectiveDeadline = ComputeEffectiveDeadlineUtc(context);
        return UnaryWithDeadlineAsync(request, context, continuation, effectiveDeadline);
    }

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        var effectiveDeadline = ComputeEffectiveDeadlineUtc(context);
        return ServerStreamingWithDeadlineAsync(request, responseStream, context, continuation, effectiveDeadline);
    }

    private static async Task<TResponse> UnaryWithDeadlineAsync<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation,
        DateTime effectiveDeadlineUtc)
        where TRequest : class
        where TResponse : class
    {
        if (IsNoFiniteDeadline(effectiveDeadlineUtc))
        {
            return await continuation(request, context).ConfigureAwait(false);
        }

        var delayRemaining = effectiveDeadlineUtc - DateTime.UtcNow;
        if (delayRemaining <= TimeSpan.Zero)
        {
            LogDeadlineExceeded(context, "Unary 入站时有效期限已过期");
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, "RPC 期限已过。"));
        }

        var workTask = continuation(request, context);
        var delayTask = Task.Delay(delayRemaining, context.CancellationToken);
        var finished = await Task.WhenAny(workTask, delayTask).ConfigureAwait(false);
        if (finished == delayTask)
        {
            LogDeadlineExceeded(context, "Unary 处理在 WhenAny 中与延迟任务竞态超时");
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, "RPC 处理超出期限。"));
        }

        return await workTask.ConfigureAwait(false);
    }

    private static async Task ServerStreamingWithDeadlineAsync<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation,
        DateTime effectiveDeadlineUtc)
        where TRequest : class
        where TResponse : class
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
        if (!IsNoFiniteDeadline(effectiveDeadlineUtc))
        {
            var delayRemaining = effectiveDeadlineUtc - DateTime.UtcNow;
            if (delayRemaining <= TimeSpan.Zero)
            {
                LogDeadlineExceeded(context, "ServerStreaming 入站时有效期限已过期");
                throw new RpcException(new Status(StatusCode.DeadlineExceeded, "RPC 期限已过。"));
            }

            cts.CancelAfter(delayRemaining);
        }

        using (RpcCallDeadlineContext.Push(cts.Token))
        {
            await continuation(request, responseStream, context).ConfigureAwait(false);
        }
    }

    private DateTime ComputeEffectiveDeadlineUtc(ServerCallContext context)
    {
        var serverNow = DateTime.UtcNow;
        var candidates = new List<DateTime>();

        if (context.Deadline < DateTime.MaxValue.AddSeconds(-1))
            candidates.Add(ToUtc(context.Deadline));

        if (TryGetHeaderTimeoutMs(context.RequestHeaders, out var metaMs) && metaMs > 0)
            candidates.Add(serverNow.AddMilliseconds(metaMs));

        if (_defaultRpcTimeoutMs > 0)
            candidates.Add(serverNow.AddMilliseconds(_defaultRpcTimeoutMs));

        if (candidates.Count == 0)
            return DateTime.MaxValue;

        return candidates.Min();
    }

    private static bool IsNoFiniteDeadline(DateTime effectiveUtc) =>
        effectiveUtc >= DateTime.MaxValue.AddYears(-1);

    private static DateTime ToUtc(DateTime deadline)
    {
        return deadline.Kind switch
        {
            DateTimeKind.Utc => deadline,
            DateTimeKind.Local => deadline.ToUniversalTime(),
            _ => DateTime.SpecifyKind(deadline, DateTimeKind.Utc),
        };
    }

    private static bool TryGetHeaderTimeoutMs(Metadata headers, out int ms)
    {
        ms = 0;
        var entry = headers.Get(TimeoutMetadataKey);
        if (entry is null)
            return false;
        if (!int.TryParse(entry.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) || v <= 0)
            return false;
        ms = v;
        return true;
    }

    /// <summary>
    /// 记录服务端判定的期限触发（与客户端可见的 <see cref="StatusCode.DeadlineExceeded"/> 一致）。
    /// </summary>
    private static void LogDeadlineExceeded(ServerCallContext context, string reason)
    {
        Logger.Warning("RPC 服务端期限触发：{Reason}，Peer={Peer}，Method={Method}", reason, context.Peer, context.Method);
    }
}
