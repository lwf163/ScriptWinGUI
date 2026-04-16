using System.Globalization;
using Google.Protobuf.Collections;
using Swg.Capture;
using Swg.Grpc.Capture;

namespace Swg.Grpc.Api;

/// <summary>
/// Capture gRPC 门面：封装 <c>Swg.Capture</c> 能力，与对应 Proto RPC 语义一致。
/// <para>
/// 提供网络流量监听窗口的创建/停止、历史记录查询等功能。
/// 所有方法均为无状态静态方法，线程安全。
/// </para>
/// <para>对应 Proto 服务：<c>swg.capture.CaptureService</c></para>
/// <para>
/// 异常映射约定（经由 <see cref="GrpcRouteRunner"/> 统一处理）：
/// <list type="bullet">
///   <item><description><see cref="ArgumentException"/> → <c>StatusCode.InvalidArgument</c></description></item>
///   <item><description><see cref="InvalidOperationException"/> → <c>StatusCode.Unavailable</c></description></item>
///   <item><description>其他异常 → <c>StatusCode.Internal</c></description></item>
/// </list>
/// </para>
/// </summary>
public static class SwgGrpcCaptureApi
{
    /// <summary>
    /// 创建一个监听窗口实例，用于捕获 HTTP 流量和/或 Windows 通知事件。
    /// <para>
    /// 监听窗口会启动本地代理服务器，拦截匹配规则的 HTTP/HTTPS 流量，
    /// 并将数据持久化到 SQLite 数据库。同时可选启用 Windows 通知捕获和 ETW 探针。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 创建监听窗口的请求参数，包含以下字段：
    /// <list type="bullet">
    ///   <item><description><c>StorageDirectory</c>（string，可选）：SQLite 数据库存储目录，为空则使用默认临时目录</description></item>
    ///   <item><description><c>ProxyListenPort</c>（int32，可选）：代理监听端口，0 或不填则自动分配</description></item>
    ///   <item><description><c>MaxBodyBytesPerPart</c>（int32，可选）：单个请求/响应体的最大字节数</description></item>
    ///   <item><description><c>FlushIntervalMs</c>（int32，可选）：批量写入数据库的刷新间隔（毫秒）</description></item>
    ///   <item><description><c>FlushBatchMaxRows</c>（int32，可选）：单批次最大行数</description></item>
    ///   <item><description><c>FlushBatchMaxBytes</c>（int64，可选）：单批次最大字节数</description></item>
    ///   <item><description><c>TrafficAllowAll</c>（bool）：是否允许捕获所有流量（不过滤）</description></item>
    ///   <item><description><c>TrafficHostContains</c>（repeated string）：流量过滤规则 - 主机名包含关键词列表</description></item>
    ///   <item><description><c>TrafficPathPrefixes</c>（repeated string）：流量过滤规则 - 路径前缀列表</description></item>
    ///   <item><description><c>MitmUserTrustRoot</c>（bool）：是否在当前用户存储安装 MITM 根证书</description></item>
    ///   <item><description><c>MitmMachineTrustRoot</c>（bool）：是否在计算机存储安装 MITM 根证书</description></item>
    ///   <item><description><c>MitmTrustRootAsAdministrator</c>（bool）：是否以管理员权限安装根证书</description></item>
    ///   <item><description><c>EnableNotifications</c>（bool）：是否启用 Windows 通知捕获</description></item>
    ///   <item><description><c>NotificationDebounceMs</c>（int32，可选）：通知事件去抖间隔（毫秒）</description></item>
    ///   <item><description><c>NotificationProcessNameContains</c>（repeated string）：通知过滤 - 进程名包含关键词</description></item>
    ///   <item><description><c>NotificationTitleContains</c>（string，可选）：通知过滤 - 标题包含关键词</description></item>
    ///   <item><description><c>HookWindowEventTypes</c>（repeated string）：窗口事件钩子类型列表</description></item>
    ///   <item><description><c>EnableEtwProbe</c>（bool）：是否启用 ETW 探针</description></item>
    ///   <item><description><c>EtwProviderNames</c>（repeated string）：ETW 音频轨提供者名称列表</description></item>
    ///   <item><description><c>EtwWindowProviderNames</c>（repeated string）：ETW 窗口轨提供者名称列表</description></item>
    ///   <item><description><c>EtwWindowEventTypes</c>（repeated string）：ETW 窗口事件类型列表</description></item>
    ///   <item><description><c>EtwMatchAnyKeyword</c>（uint64，可选）：ETW 关键字掩码</description></item>
    ///   <item><description><c>EtwLevel</c>（uint32，可选）：ETW 事件级别</description></item>
    ///   <item><description><c>EtwQueueCapacity</c>（int32，可选）：ETW 音频轨队列容量</description></item>
    ///   <item><description><c>EtwWindowQueueCapacity</c>（int32，可选）：ETW 窗口轨队列容量</description></item>
    ///   <item><description><c>TrafficStatsIntervalMs</c>（int32，可选）：流量统计上报间隔（毫秒）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="CaptureCreateListenWindowResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>ListenWindowId</c>（string）：监听窗口的唯一标识（GUID 格式），后续操作需使用此 ID</description></item>
    ///   <item><description><c>SqlitePath</c>（string）：SQLite 数据库文件完整路径</description></item>
    ///   <item><description><c>ProxyListenPort</c>（int32）：代理实际监听的端口号</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException">
    /// 参数验证失败时抛出，常见场景：
    /// <list type="bullet">
    ///   <item><description>EtwWindowProviderNames 与 EtwWindowEventTypes 未同时提供或同时省略</description></item>
    ///   <item><description>HookWindowEventTypes 包含无效的事件类型名称</description></item>
    /// </list>
    /// </exception>
    public static CaptureCreateListenWindowResponse CreateListenWindow(CaptureCreateListenWindowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ListenWindowOptions opt = MapOptions(request);
        ListenWindow window = ListenWindowManager.Default.Create(opt);
        return new CaptureCreateListenWindowResponse
        {
            ListenWindowId = window.Id.ToString("D", CultureInfo.InvariantCulture),
            SqlitePath = window.SqlitePath,
            ProxyListenPort = window.ProxyListenPort,
        };
    }

    /// <summary>
    /// 停止并销毁指定监听窗口，释放代理端口和所有关联资源。
    /// </summary>
    /// <param name="request">
    /// 停止监听窗口的请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ListenWindowId</c>（string，必填）：由 <see cref="CreateListenWindow"/> 返回的监听窗口 ID</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="CaptureStopListenWindowResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Stopped</c>（bool）：是否成功停止（若 ID 不存在则为 false）</description></item>
    ///   <item><description><c>SqlitePath</c>（string）：已关闭窗口对应的 SQLite 数据库路径，未停止时为空字符串</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static CaptureStopListenWindowResponse StopListenWindow(CaptureStopListenWindowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guid id = Guid.Parse(request.ListenWindowId);
        bool ok = ListenWindowManager.Default.TryStop(id, out string? path);
        return new CaptureStopListenWindowResponse
        {
            Stopped = ok,
            SqlitePath = path ?? "",
        };
    }

    /// <summary>
    /// 分页查询指定监听窗口捕获的 HTTP 交换记录。
    /// <para>
    /// 返回结果按捕获时间倒序排列，支持基于时间戳的分页游标。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 查询历史记录的请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ListenWindowId</c>（string，必填）：监听窗口 ID</description></item>
    ///   <item><description><c>Limit</c>（int32）：单页最大返回条数</description></item>
    ///   <item><description><c>Offset</c>（int32）：偏移量</description></item>
    ///   <item><description><c>BeforeCapturedAtUtc</c>（string，可选）：ISO 8601 时间戳，仅返回此时间之前捕获的记录</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="CaptureHistoryQueryResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Items</c>（repeated <see cref="CaptureHttpExchangeItem"/>）：HTTP 交换记录列表</description></item>
    /// </list>
    /// <para>每条 <see cref="CaptureHttpExchangeItem"/> 包含：</para>
    /// <list type="bullet">
    ///   <item><description><c>Id</c>（int64）：记录唯一 ID</description></item>
    ///   <item><description><c>CapturedAt</c>（string）：捕获时间（ISO 8601 格式）</description></item>
    ///   <item><description><c>Method</c>（string）：HTTP 方法（GET/POST 等）</description></item>
    ///   <item><description><c>Scheme</c>（string）：协议（http/https）</description></item>
    ///   <item><description><c>Host</c>（string）：目标主机名</description></item>
    ///   <item><description><c>Port</c>（int32）：目标端口</description></item>
    ///   <item><description><c>Path</c>（string）：请求路径</description></item>
    ///   <item><description><c>QueryText</c>（string）：查询字符串</description></item>
    ///   <item><description><c>UrlDisplay</c>（string）：用于展示的完整 URL</description></item>
    ///   <item><description><c>ResponseStatus</c>（int32）：HTTP 响应状态码（<c>HasResponseStatus</c> 为 true 时有效）</description></item>
    ///   <item><description><c>DurationMs</c>（int32）：请求耗时毫秒数（<c>HasDurationMs</c> 为 true 时有效）</description></item>
    ///   <item><description><c>ErrorText</c>（string）：错误信息（无错误时为空）</description></item>
    ///   <item><description><c>ClientProcessId</c>（uint32）：发起请求的客户端进程 ID（<c>HasClientProcessId</c> 为 true 时有效）</description></item>
    ///   <item><description><c>ClientProcessName</c>（string）：发起请求的客户端进程名称</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="InvalidOperationException">指定的 <c>ListenWindowId</c> 不存在或已停止</exception>
    public static CaptureHistoryQueryResponse QueryHistory(CaptureHistoryQueryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        DateTimeOffset? before = null;
        if (!string.IsNullOrWhiteSpace(request.BeforeCapturedAtUtc))
        {
            before = DateTimeOffset.Parse(
                request.BeforeCapturedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        Guid listenId = Guid.Parse(request.ListenWindowId);
        ListenWindow w = ListenWindowManager.Default.GetOrThrow(listenId);
        IReadOnlyList<HttpExchangeRecord> rows = w.QueryHistoryMerged(request.Limit, request.Offset, before);

        var r = new CaptureHistoryQueryResponse();
        foreach (HttpExchangeRecord x in rows)
        {
            var item = new CaptureHttpExchangeItem
            {
                Id = x.Id,
                CapturedAt = x.CapturedAt.ToString("O", CultureInfo.InvariantCulture),
                Method = x.Method,
                Scheme = x.Scheme,
                Host = x.Host,
                Port = x.Port,
                Path = x.Path,
                QueryText = x.QueryText ?? "",
                UrlDisplay = x.UrlDisplay,
                ErrorText = x.ErrorText ?? "",
                ClientProcessName = x.ClientProcessName ?? "",
            };
            if (x.ResponseStatus.HasValue)
            {
                item.HasResponseStatus = true;
                item.ResponseStatus = x.ResponseStatus.Value;
            }

            if (x.DurationMs.HasValue)
            {
                item.HasDurationMs = true;
                item.DurationMs = x.DurationMs.Value;
            }

            if (x.ClientProcessId.HasValue)
            {
                item.HasClientProcessId = true;
                item.ClientProcessId = (uint)x.ClientProcessId.Value;
            }

            r.Items.Add(item);
        }

        return r;
    }

    private static ListenWindowOptions MapOptions(CaptureCreateListenWindowRequest body)
    {
        var opt = new ListenWindowOptions();

        if (!string.IsNullOrWhiteSpace(body.StorageDirectory))
            opt.StorageDirectory = body.StorageDirectory.Trim();

        if (body.ProxyListenPort > 0)
            opt.ProxyListenPort = body.ProxyListenPort;

        if (body.MaxBodyBytesPerPart > 0)
            opt.MaxBodyBytesPerPart = body.MaxBodyBytesPerPart;

        if (body.FlushIntervalMs > 0)
            opt.FlushIntervalMs = body.FlushIntervalMs;

        if (body.FlushBatchMaxRows > 0)
            opt.FlushBatchMaxRows = body.FlushBatchMaxRows;

        if (body.FlushBatchMaxBytes > 0)
            opt.FlushBatchMaxBytes = body.FlushBatchMaxBytes;

        opt.TrafficFilter = new HttpCaptureFilterRules
        {
            AllowAll = body.TrafficAllowAll,
            HostContains = body.TrafficHostContains.Count > 0 ? body.TrafficHostContains.ToList() : [],
            PathPrefixes = body.TrafficPathPrefixes.Count > 0 ? body.TrafficPathPrefixes.ToList() : [],
        };

        opt.Mitm = new MitmCertificateOptions
        {
            UserTrustRoot = body.MitmUserTrustRoot,
            MachineTrustRoot = body.MitmMachineTrustRoot,
            TrustRootAsAdministrator = body.MitmTrustRootAsAdministrator,
        };

        opt.EnableNotifications = body.EnableNotifications;

        if (body.NotificationDebounceMs > 0)
            opt.Notification.DebounceMs = body.NotificationDebounceMs;

        opt.Notification.ProcessNameContains = body.NotificationProcessNameContains.Count > 0
            ? body.NotificationProcessNameContains.ToList()
            : [];
        opt.Notification.TitleContains = string.IsNullOrWhiteSpace(body.NotificationTitleContains)
            ? null
            : body.NotificationTitleContains.Trim();

        if (body.HookWindowEventTypes.Count > 0)
            opt.Notification.HookWindowEventTypes = body.HookWindowEventTypes.ToList();

        opt.EnableEtwProbe = body.EnableEtwProbe;

        if (body.EtwProviderNames.Count > 0)
            opt.Etw.ProviderNames = body.EtwProviderNames.ToList();

        if (body.EtwWindowProviderNames.Count > 0)
            opt.Etw.WindowProviderNames = body.EtwWindowProviderNames.ToList();

        if (body.EtwWindowEventTypes.Count > 0)
            opt.Etw.WindowEventTypes = body.EtwWindowEventTypes.ToList();

        if (body.HasEtwMatchAnyKeyword)
            opt.Etw.MatchAnyKeyword = body.EtwMatchAnyKeyword;

        if (body.HasEtwLevel)
            opt.Etw.Level = (byte)body.EtwLevel;

        if (body.EtwQueueCapacity > 0)
            opt.Etw.QueueCapacity = body.EtwQueueCapacity;

        if (body.EtwWindowQueueCapacity > 0)
            opt.Etw.WindowQueueCapacity = body.EtwWindowQueueCapacity;

        if (body.TrafficStatsIntervalMs > 0)
            opt.TrafficStatsIntervalMs = body.TrafficStatsIntervalMs;

        ValidateCaptureOptions(opt, body);
        return opt;
    }

    private static void ValidateCaptureOptions(ListenWindowOptions opt, CaptureCreateListenWindowRequest body)
    {
        if (opt.EnableNotifications && body.HookWindowEventTypes.Count > 0)
        {
            IReadOnlyList<string> hook = WindowCaptureEventTypes.NormalizeSubscription(body.HookWindowEventTypes);
            if (hook.Count > 0)
            {
                WindowCaptureEventTypes.ValidateSubscription(
                    hook.ToList(),
                    nameof(body.HookWindowEventTypes));
            }
        }

        if (!opt.EnableEtwProbe)
            return;

        static bool HasNonEmpty(RepeatedField<string> list) =>
            list.Count > 0 && list.Any(static s => !string.IsNullOrWhiteSpace(s));

        bool hasAudio = HasNonEmpty(body.EtwProviderNames);
        bool hasWinP = HasNonEmpty(body.EtwWindowProviderNames);
        bool hasWinT = body.EtwWindowEventTypes.Count > 0 &&
                       WindowCaptureEventTypes.NormalizeSubscription(body.EtwWindowEventTypes).Count > 0;

        if (!hasAudio && !hasWinP && !hasWinT)
            return;

        if (hasWinP != hasWinT)
        {
            throw new ArgumentException(
                "EtwWindowProviderNames 与 EtwWindowEventTypes 必须同时提供或同时省略（可与 EtwProviderNames 音频轨并存）。");
        }

        if (hasWinT)
        {
            IReadOnlyList<string> wt = WindowCaptureEventTypes.NormalizeSubscription(body.EtwWindowEventTypes);
            WindowCaptureEventTypes.ValidateSubscription(
                wt.ToList(),
                nameof(body.EtwWindowEventTypes));
        }
    }
}
