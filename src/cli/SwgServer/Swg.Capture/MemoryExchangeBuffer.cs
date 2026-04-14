namespace Swg.Capture;

/// <summary>
/// 线程安全的内存交换缓冲：入队、出队批量刷盘、未刷盘快照查询。
/// </summary>
public sealed class MemoryExchangeBuffer
{
    private readonly object _lock = new();
    private readonly List<HttpExchangeRecord> _pending = new();

    public void Enqueue(HttpExchangeRecord row)
    {
        lock (_lock)
        {
            _pending.Add(row);
        }
    }

    /// <summary>取出当前待刷盘项并清空（由刷盘线程调用）。</summary>
    public IReadOnlyList<HttpExchangeRecord> DrainAndClear()
    {
        lock (_lock)
        {
            if (_pending.Count == 0)
                return Array.Empty<HttpExchangeRecord>();

            HttpExchangeRecord[] copy = _pending.ToArray();
            _pending.Clear();
            return copy;
        }
    }

    /// <summary>未刷盘快照（用于历史分页合并）。</summary>
    public IReadOnlyList<HttpExchangeRecord> SnapshotPending()
    {
        lock (_lock)
        {
            return _pending.ToArray();
        }
    }

    public int PendingCount
    {
        get
        {
            lock (_lock)
            {
                return _pending.Count;
            }
        }
    }
}
