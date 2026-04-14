using Grpc.Core;
using Serilog;

namespace Swg.Grpc;

/// <summary>
/// 将业务异常映射为 gRPC <see cref="Status"/>（<c>ArgumentException</c>→<see cref="StatusCode.InvalidArgument"/>，<c>InvalidOperationException</c>→<see cref="StatusCode.Unavailable"/>，其余→<see cref="StatusCode.Internal"/>）。
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
        catch (RpcException)
        {
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
        catch (RpcException)
        {
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
        catch (RpcException)
        {
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
        catch (Exception ex)
        {
            Logger.Error(ex, "gRPC 路由映射为 Internal");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
