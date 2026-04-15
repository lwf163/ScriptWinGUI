using Grpc.Core;
using Serilog;

namespace Swg.Grpc;

/// <summary>
/// 将业务异常映射为 gRPC <see cref="Status"/>（<c>ArgumentException</c>→<see cref="StatusCode.InvalidArgument"/>，<c>InvalidOperationException</c>→<see cref="StatusCode.Unavailable"/>，
/// <c>OperationCanceledException</c>→<see cref="StatusCode.Cancelled"/>，<c>TimeoutException</c>→<see cref="StatusCode.DeadlineExceeded"/>，其余→<see cref="StatusCode.Internal"/>）。
/// 已构造的 <see cref="RpcException"/> 以 Debug 级别记录后原样抛出。
/// </summary>
public static class GrpcRouteRunner
{
    private static readonly ILogger Logger = Log.ForContext(typeof(GrpcRouteRunner));

    public static T Run<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (RpcException ex)
        {
            Logger.Debug(ex, "gRPC 路由透传 RpcException：{GrpcStatusCode} {GrpcStatusDetail}", ex.Status.StatusCode, ex.Status.Detail);
            throw;
        }
        catch (ArgumentException ex)
        {
            Logger.Debug(ex, "gRPC 路由映射为 InvalidArgument");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            Logger.Warning(ex, "gRPC 路由映射为 Unavailable");
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }
        catch (OperationCanceledException ex)
        {
            Logger.Debug(ex, "gRPC 路由映射为 Cancelled");
            throw new RpcException(new Status(StatusCode.Cancelled, ex.Message));
        }
        catch (TimeoutException ex)
        {
            Logger.Debug(ex, "gRPC 路由映射为 DeadlineExceeded");
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, ex.Message));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "gRPC 路由映射为 Internal");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public static async Task<T> RunAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (RpcException ex)
        {
            Logger.Debug(ex, "gRPC 路由透传 RpcException：{GrpcStatusCode} {GrpcStatusDetail}", ex.Status.StatusCode, ex.Status.Detail);
            throw;
        }
        catch (ArgumentException ex)
        {
            Logger.Debug(ex, "gRPC 路由映射为 InvalidArgument");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            Logger.Warning(ex, "gRPC 路由映射为 Unavailable");
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }
        catch (OperationCanceledException ex)
        {
            Logger.Debug(ex, "gRPC 路由映射为 Cancelled");
            throw new RpcException(new Status(StatusCode.Cancelled, ex.Message));
        }
        catch (TimeoutException ex)
        {
            Logger.Debug(ex, "gRPC 路由映射为 DeadlineExceeded");
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, ex.Message));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "gRPC 路由映射为 Internal");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public static async Task RunAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (RpcException ex)
        {
            Logger.Debug(ex, "gRPC 路由透传 RpcException：{GrpcStatusCode} {GrpcStatusDetail}", ex.Status.StatusCode, ex.Status.Detail);
            throw;
        }
        catch (ArgumentException ex)
        {
            Logger.Debug(ex, "gRPC 路由映射为 InvalidArgument");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            Logger.Warning(ex, "gRPC 路由映射为 Unavailable");
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }
        catch (OperationCanceledException ex)
        {
            Logger.Debug(ex, "gRPC 路由映射为 Cancelled");
            throw new RpcException(new Status(StatusCode.Cancelled, ex.Message));
        }
        catch (TimeoutException ex)
        {
            Logger.Debug(ex, "gRPC 路由映射为 DeadlineExceeded");
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, ex.Message));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "gRPC 路由映射为 Internal");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
