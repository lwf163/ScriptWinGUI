using Serilog;

namespace Swg.Capture;

/// <summary>
/// 通知侧 WebSocket 广播入口（与 <see cref="TrafficPushHub"/> 无任何共享连接或广播器状态）。
/// </summary>
public static class NotificationPushHub
{
    private static readonly ILogger Logger = Log.ForContext(typeof(NotificationPushHub));

    private static Func<string, Task>? _broadcast;

    private static readonly object _subscriberLock = new();
    private static readonly List<Func<string, Task>> _subscribers = [];

    public static void RegisterBroadcast(Func<string, Task> broadcast)
    {
        _broadcast = broadcast;
    }

    public static void Clear()
    {
        _broadcast = null;
    }

    /// <summary>
    /// 额外订阅（如 gRPC Server streaming）；与 <see cref="RegisterBroadcast"/> 并行收到同一份 JSON。
    /// </summary>
    public static IDisposable Subscribe(Func<string, Task> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_subscriberLock)
        {
            _subscribers.Add(listener);
        }

        return new SubscriptionRemover(() =>
        {
            lock (_subscriberLock)
            {
                _ = _subscribers.Remove(listener);
            }
        });
    }

    public static async Task PushJsonAsync(string json)
    {
        List<Func<string, Task>> copy;
        Func<string, Task>? b;
        lock (_subscriberLock)
        {
            copy = [.. _subscribers];
        }

        b = _broadcast;

        foreach (Func<string, Task> s in copy)
        {
            try
            {
                await s(json).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "通知侧订阅者推送 JSON 失败");
            }
        }

        if (b is not null)
        {
            try
            {
                await b(json).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "通知侧 RegisterBroadcast 推送 JSON 失败");
            }
        }
    }

    private sealed class SubscriptionRemover : IDisposable
    {
        private readonly Action _remove;
        private bool _disposed;

        public SubscriptionRemover(Action remove) => _remove = remove;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _remove();
        }
    }
}

/// <summary>
/// 流量侧 WebSocket 广播入口（与 <see cref="NotificationPushHub"/> 隔离）。
/// </summary>
public static class TrafficPushHub
{
    private static readonly ILogger Logger = Log.ForContext(typeof(TrafficPushHub));

    private static Func<string, Task>? _broadcast;

    private static readonly object _subscriberLock = new();
    private static readonly List<Func<string, Task>> _subscribers = [];

    public static void RegisterBroadcast(Func<string, Task> broadcast)
    {
        _broadcast = broadcast;
    }

    public static void Clear()
    {
        _broadcast = null;
    }

    /// <summary>
    /// 额外订阅（如 gRPC Server streaming）；与 <see cref="RegisterBroadcast"/> 并行收到同一份 JSON。
    /// </summary>
    public static IDisposable Subscribe(Func<string, Task> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_subscriberLock)
        {
            _subscribers.Add(listener);
        }

        return new SubscriptionRemover(() =>
        {
            lock (_subscriberLock)
            {
                _ = _subscribers.Remove(listener);
            }
        });
    }

    public static async Task PushJsonAsync(string json)
    {
        List<Func<string, Task>> copy;
        Func<string, Task>? b;
        lock (_subscriberLock)
        {
            copy = [.. _subscribers];
        }

        b = _broadcast;

        foreach (Func<string, Task> s in copy)
        {
            try
            {
                await s(json).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "流量侧订阅者推送 JSON 失败");
            }
        }

        if (b is not null)
        {
            try
            {
                await b(json).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "流量侧 RegisterBroadcast 推送 JSON 失败");
            }
        }
    }

    private sealed class SubscriptionRemover : IDisposable
    {
        private readonly Action _remove;
        private bool _disposed;

        public SubscriptionRemover(Action remove) => _remove = remove;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _remove();
        }
    }
}
