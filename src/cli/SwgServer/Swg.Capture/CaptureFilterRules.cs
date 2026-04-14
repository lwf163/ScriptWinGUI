namespace Swg.Capture;

/// <summary>
/// HTTP(S) 捕获过滤规则：默认放行全部；若 <see cref="AllowAll"/> 为 false，则至少匹配一项白名单规则才捕获。
/// </summary>
public sealed class HttpCaptureFilterRules
{
    /// <summary>为 true 时不应用下方白名单（全部捕获）。</summary>
    public bool AllowAll { get; set; } = true;

    /// <summary>主机名包含任一子串则匹配（忽略大小写）。</summary>
    public IReadOnlyList<string>? HostContains { get; set; }

    /// <summary>路径前缀匹配（忽略大小写）。</summary>
    public IReadOnlyList<string>? PathPrefixes { get; set; }

    public bool IsAllowed(Uri uri)
    {
        if (AllowAll)
            return true;

        string host = uri.Host;
        string path = uri.AbsolutePath;

        if (HostContains is { Count: > 0 })
        {
            foreach (string fragment in HostContains)
            {
                if (host.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        if (PathPrefixes is { Count: > 0 })
        {
            foreach (string prefix in PathPrefixes)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }
}
