using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Serilog;
using Swg.Capture;
using Swg.Grpc.Api;
using Swg.Grpc.Capture;

namespace Swg.Grpc.Services;

public sealed class CaptureGrpcService : CaptureService.CaptureServiceBase
{
    private static readonly ILogger Logger = Log.ForContext(typeof(CaptureGrpcService));

    public override Task<CaptureCreateListenWindowResponse> CreateListenWindow(CaptureCreateListenWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCaptureApi.CreateListenWindow(request)));

    public override Task<CaptureStopListenWindowResponse> StopListenWindow(CaptureStopListenWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCaptureApi.StopListenWindow(request)));

    public override Task<CaptureHistoryQueryResponse> QueryHistory(CaptureHistoryQueryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCaptureApi.QueryHistory(request)));

    public override async Task SubscribeNotifications(Empty request, IServerStreamWriter<CaptureNotificationEvent> responseStream, ServerCallContext context)
    {
        using IDisposable sub = NotificationPushHub.Subscribe(async json =>
        {
            await responseStream.WriteAsync(new CaptureNotificationEvent { JsonPayload = json }).ConfigureAwait(false);
        });
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, context.CancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // 客户端取消或连接结束
            Logger.Debug("Capture 通知流结束（取消或断开）");
        }
    }

    public override async Task SubscribeTraffic(Empty request, IServerStreamWriter<TrafficChunk> responseStream, ServerCallContext context)
    {
        using IDisposable sub = TrafficPushHub.Subscribe(async json =>
        {
            await responseStream.WriteAsync(new TrafficChunk { JsonPayload = json }).ConfigureAwait(false);
        });
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, context.CancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Logger.Debug("Capture 流量流结束（取消或断开）");
        }
    }
}
