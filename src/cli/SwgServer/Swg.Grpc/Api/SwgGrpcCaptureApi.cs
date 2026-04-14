using System.Globalization;
using Google.Protobuf.Collections;
using Swg.Capture;
using Swg.Grpc.Capture;

namespace Swg.Grpc.Api;

/// <summary>
/// Capture gRPC 门面：封装 <c>Swg.Capture</c> 能力，与对应 Proto RPC 语义一致。
/// </summary>
public static class SwgGrpcCaptureApi
{
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
