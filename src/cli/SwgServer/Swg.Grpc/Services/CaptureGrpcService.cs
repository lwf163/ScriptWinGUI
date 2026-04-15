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
        try
        {
            using IDisposable sub = NotificationPushHub.Subscribe(async json =>
            {
                try
                {
                    await responseStream.WriteAsync(new CaptureNotificationEvent { JsonPayload = json }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // 记录后吞掉，避免推送路径上未观察任务异常；长流保持直至 Delay 侧取消
                    Logger.Error(ex, "Capture 通知流向客户端写入失败");
                }
            });
            await Task.Delay(Timeout.InfiniteTimeSpan, RpcCallDeadlineContext.GetEffectiveToken(context)).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            Logger.Debug(ex, "Capture 通知流结束（取消、期限或断开）");
            throw new RpcException(new Status(StatusCode.Cancelled, ex.Message));
        }
        catch (RpcException ex)
        {
            Logger.Debug(ex, "Capture 通知流透传 RpcException：{GrpcStatusCode}", ex.Status.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Capture 通知流未预期异常");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task SubscribeTraffic(Empty request, IServerStreamWriter<TrafficChunk> responseStream, ServerCallContext context)
    {
        try
        {
            using IDisposable sub = TrafficPushHub.Subscribe(async json =>
            {
                try
                {
                    await responseStream.WriteAsync(new TrafficChunk { JsonPayload = json }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Capture 流量流向客户端写入失败");
                }
            });
            await Task.Delay(Timeout.InfiniteTimeSpan, RpcCallDeadlineContext.GetEffectiveToken(context)).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            Logger.Debug(ex, "Capture 流量流结束（取消、期限或断开）");
            throw new RpcException(new Status(StatusCode.Cancelled, ex.Message));
        }
        catch (RpcException ex)
        {
            Logger.Debug(ex, "Capture 流量流透传 RpcException：{GrpcStatusCode}", ex.Status.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Capture 流量流未预期异常");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
