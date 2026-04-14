namespace Swg.Capture;

/// <summary>
/// 管理多个监听窗口（逻辑会话）的创建与销毁。
/// </summary>
public sealed class ListenWindowManager
{
    public static ListenWindowManager Default { get; } = new();

    private readonly object _sync = new();
    private readonly Dictionary<Guid, ListenWindow> _windows = new();

    public ListenWindow Create(ListenWindowOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var id = Guid.NewGuid();
        string fileName = $"{id:N}_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.sqlite";
        string path = Path.Combine(options.StorageDirectory, fileName);

        var window = new ListenWindow(id, options, path);
        lock (_sync)
        {
            _windows[id] = window;
        }

        return window;
    }

    public ListenWindow GetOrThrow(Guid id)
    {
        lock (_sync)
        {
            if (_windows.TryGetValue(id, out ListenWindow? w))
                return w;
        }

        throw new ArgumentException("ListenWindowId 不存在。");
    }

    public bool TryStop(Guid id, out string? sqlitePath)
    {
        lock (_sync)
        {
            if (!_windows.Remove(id, out ListenWindow? w))
            {
                sqlitePath = null;
                return false;
            }

            sqlitePath = w.SqlitePath;
            w.Dispose();
            return true;
        }
    }
}
