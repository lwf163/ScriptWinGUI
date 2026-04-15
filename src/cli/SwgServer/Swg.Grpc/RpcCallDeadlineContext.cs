using Grpc.Core;

namespace Swg.Grpc;

/// <summary>
/// 由宿主侧拦截器在流式 RPC 上推送「合并后的取消令牌」（客户端取消 + 服务端期限），供服务实现中与 <see cref="ServerCallContext.CancellationToken"/> 等价的替代用法。
/// Unary 调用未推送时，<see cref="GetEffectiveToken"/> 回退为上下文自带令牌。
/// </summary>
public static class RpcCallDeadlineContext
{
    private static readonly AsyncLocal<CancellationToken?> LocalToken = new();

    /// <summary>
    /// 推送当前调用应使用的取消令牌；返回的对象需在调用结束后释放以恢复外层上下文。
    /// </summary>
    public static IDisposable Push(CancellationToken mergedToken)
    {
        var prior = LocalToken.Value;
        LocalToken.Value = mergedToken;
        return new PopDisposable(() => LocalToken.Value = prior);
    }

    /// <summary>
    /// 返回流式 RPC 合并期限后的令牌；若未处于推送作用域则使用 <paramref name="context"/> 的令牌。
    /// </summary>
    public static CancellationToken GetEffectiveToken(ServerCallContext context) =>
        LocalToken.Value ?? context.CancellationToken;

    private sealed class PopDisposable : IDisposable
    {
        private readonly Action _pop;
        private bool _disposed;

        public PopDisposable(Action pop) => _pop = pop;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _pop();
        }
    }
}
