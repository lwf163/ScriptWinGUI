using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading;
using Serilog;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace Swg.Capture;

/// <summary>
/// 基于 Unobtanium/Titanium 的本地代理：将请求—响应映射为 <see cref="HttpExchangeRecord"/> 并写入缓冲与过滤。
/// </summary>
public sealed class HttpProxyPipeline : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext(typeof(HttpProxyPipeline));

    private readonly Guid _listenWindowId;
    private readonly MemoryExchangeBuffer _buffer;
    private readonly HttpCaptureFilterRules _filter;
    private readonly int _maxBodyBytes;
    private readonly ProxyServer _server;
    private readonly ExplicitProxyEndPoint _endPoint;
    private bool _disposed;

    public HttpProxyPipeline(
        Guid listenWindowId,
        ListenWindowOptions options,
        MemoryExchangeBuffer buffer)
    {
        _listenWindowId = listenWindowId;
        _buffer = buffer;
        _filter = options.TrafficFilter;
        _maxBodyBytes = Math.Max(1, options.MaxBodyBytesPerPart);

        _server = new ProxyServer();
        MitmCertificateHelper.Apply(_server, options.Mitm);

        _endPoint = new ExplicitProxyEndPoint(IPAddress.Loopback, options.ProxyListenPort, decryptSsl: true);
        _server.AddEndPoint(_endPoint);
        _server.BeforeRequest += OnBeforeRequest;
        _server.AfterResponse += OnAfterResponseAsync;
    }

    public int ListeningPort => _endPoint.Port;

    public void Start()
    {
        _server.StartAsync(false, CancellationToken.None).GetAwaiter().GetResult();
    }

    private Task OnBeforeRequest(object? sender, SessionEventArgs e)
    {
        e.UserData = Stopwatch.StartNew();
        return Task.CompletedTask;
    }

    private async Task OnAfterResponseAsync(object? sender, SessionEventArgs e)
    {
        try
        {
            Request? preReq = e.HttpClient?.Request;
            Uri? preUri = ResolveRequestUri(preReq);
            if (!_filter.AllowAll && preUri is null)
                return;
            if (preUri is not null && !_filter.IsAllowed(preUri))
                return;

            HttpExchangeRecord row = await BuildRecordAsync(e).ConfigureAwait(false);

            _buffer.Enqueue(row);
            string json = TrafficEventSerializer.ExchangeSummary(_listenWindowId, row);
            await TrafficPushHub.PushJsonAsync(json).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "HTTP 代理捕获会话后处理失败，已写入 error://proxy");
            var err = new HttpExchangeRecord
            {
                CapturedAt = DateTimeOffset.UtcNow,
                Method = "GET",
                Scheme = "http",
                Host = "",
                Port = 80,
                Path = "/",
                UrlDisplay = "error://proxy",
                ErrorText = ex.Message,
            };
            _buffer.Enqueue(err);
            await TrafficPushHub.PushJsonAsync(TrafficEventSerializer.ExchangeSummary(_listenWindowId, err)).ConfigureAwait(false);
        }
    }

    private async Task<HttpExchangeRecord> BuildRecordAsync(SessionEventArgs e)
    {
        var row = new HttpExchangeRecord { CapturedAt = DateTimeOffset.UtcNow };

        HttpWebClient? client = e.HttpClient;
        Request? req = client?.Request;
        Response? resp = client?.Response;

        if (client is not null)
        {
            int pid = client.ProcessId.Value;
            if (pid > 0)
            {
                row.ClientProcessId = pid;
                try
                {
                    using Process p = Process.GetProcessById(pid);
                    row.ClientProcessName = p.ProcessName;
                }
                catch
                {
                    // 进程已退出等情况
                }
            }
        }

        if (req is not null)
        {
            Uri? uri = ResolveRequestUri(req);
            if (uri is null)
            {
                row.UrlDisplay = "unknown://session";
                return row;
            }

            row.Method = req.Method ?? "";
            row.Scheme = uri.Scheme;
            row.Host = uri.Host;
            row.Port = uri.Port;
            row.Path = uri.AbsolutePath;
            row.QueryText = string.IsNullOrEmpty(uri.Query) ? null : uri.Query.TrimStart('?');
            row.UrlDisplay = uri.ToString();
            row.RequestHeadersJson = SerializeHeaders(req.Headers);

            try
            {
                byte[] body = await e.GetRequestBody(CancellationToken.None).ConfigureAwait(false);
                (row.RequestBodyBlob, row.RequestBodyLength, row.RequestBodyTruncated) = TruncateBody(body, _maxBodyBytes);
            }
            catch
            {
                // body 未缓存或不可读
            }
        }
        else
        {
            row.UrlDisplay = "unknown://session";
        }

        if (resp is not null)
        {
            row.ResponseStatus = resp.StatusCode;
            row.ResponseHeadersJson = SerializeHeaders(resp.Headers);

            try
            {
                byte[] body = await e.GetResponseBody(CancellationToken.None).ConfigureAwait(false);
                (row.ResponseBodyBlob, row.ResponseBodyLength, row.ResponseBodyTruncated) = TruncateBody(body, _maxBodyBytes);
            }
            catch
            {
                // body 未缓存或不可读
            }
        }

        if (e.UserData is Stopwatch sw)
        {
            row.DurationMs = (int)Math.Min(int.MaxValue, sw.ElapsedMilliseconds);
        }

        return row;
    }

    private static Uri? ResolveRequestUri(Request? req)
    {
        if (req is null)
            return null;

        if (req.RequestUri is not null)
            return req.RequestUri;

        return string.IsNullOrEmpty(req.Url) ? null : new Uri(req.Url);
    }

    private static string? SerializeHeaders(HeaderCollection? headers)
    {
        if (headers is null)
            return null;

        IReadOnlyList<HttpHeader> all = headers.GetAllHeaders();
        if (all.Count == 0)
            return null;

        var pairs = new List<(string name, string value)>(all.Count);
        foreach (HttpHeader h in all)
        {
            pairs.Add((h.Name, h.Value));
        }

        return JsonSerializer.Serialize(pairs, CaptureJson.Options);
    }

    private static (byte[]? blob, int length, int truncated) TruncateBody(byte[]? body, int maxBytes)
    {
        if (body is null || body.Length == 0)
            return (null, 0, 0);

        int len = body.Length;
        if (len <= maxBytes)
            return (body, len, 0);

        byte[] copy = new byte[maxBytes];
        Buffer.BlockCopy(body, 0, copy, 0, maxBytes);
        return (copy, len, 1);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _server.BeforeRequest -= OnBeforeRequest;
        _server.AfterResponse -= OnAfterResponseAsync;
        _server.Stop();
        _server.Dispose();
    }
}
