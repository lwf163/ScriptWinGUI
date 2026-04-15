using Grpc.Core;
using Serilog;

namespace Swg.Grpc;

/// <summary>
/// 进程内全局互斥：同一时刻仅允许一个 RPC 路径执行会驱动 Windows 鼠标或键盘输入的逻辑。
/// </summary>
public static class WindowsGlobalInputGate
{
    private static readonly ILogger Logger = Log.ForContext(typeof(WindowsGlobalInputGate));

    private static readonly SemaphoreSlim Gate = new(1, 1);

    /// <summary>
    /// 在持锁状态下执行异步操作；在 <paramref name="context"/> 取消时退出等待。
    /// </summary>
    public static async Task<T> RunAsync<T>(ServerCallContext context, Func<Task<T>> action)
    {
        try
        {
            await Gate.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            Logger.Debug(ex, "等待全局输入锁时取消");
            throw;
        }

        try
        {
            return await action().ConfigureAwait(false);
        }
        finally
        {
            Gate.Release();
        }
    }

    /// <summary>
    /// 在持锁状态下执行异步操作（无返回值）。
    /// </summary>
    public static async Task RunAsync(ServerCallContext context, Func<Task> action)
    {
        try
        {
            await Gate.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            Logger.Debug(ex, "等待全局输入锁时取消");
            throw;
        }

        try
        {
            await action().ConfigureAwait(false);
        }
        finally
        {
            Gate.Release();
        }
    }
}
