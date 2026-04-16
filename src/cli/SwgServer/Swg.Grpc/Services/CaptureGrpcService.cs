using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Serilog;
using Swg.Capture;
using Swg.Grpc.Api;
using Swg.Grpc.Capture;

namespace Swg.Grpc.Services;

/// <summary>
/// Capture gRPC 服务实现：提供网络流量监听窗口管理、历史记录查询和实时事件流订阅。
/// <para>
/// 继承自 <c>CaptureService.CaptureServiceBase</c>，由 gRPC 运行时自动注册。
/// 所有一元 RPC 均通过 <see cref="GrpcRouteRunner"/> 统一异常映射；
/// 服务端流 RPC（SubscribeNotifications、SubscribeTraffic）自行处理异常映射。
/// </para>
/// <para>对应 Proto 定义：<c>swg.capture.CaptureService</c></para>
/// </summary>
public sealed class CaptureGrpcService : CaptureService.CaptureServiceBase
{
    private static readonly ILogger Logger = Log.ForContext(typeof(CaptureGrpcService));

    /// <summary>
    /// 创建一个监听窗口实例，启动代理服务器和数据捕获。
    /// </summary>
    public override Task<CaptureCreateListenWindowResponse> CreateListenWindow(CaptureCreateListenWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCaptureApi.CreateListenWindow(request)));

    /// <summary>
    /// 停止并销毁指定监听窗口，释放代理端口和所有关联资源。
    /// </summary>
    public override Task<CaptureStopListenWindowResponse> StopListenWindow(CaptureStopListenWindowRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCaptureApi.StopListenWindow(request)));

    /// <summary>
    /// 分页查询指定监听窗口捕获的 HTTP 交换记录。
    /// </summary>
    public override Task<CaptureHistoryQueryResponse> QueryHistory(CaptureHistoryQueryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcCaptureApi.QueryHistory(request)));

    /// <summary>
    /// 订阅 Windows 通知事件流（服务端流式 RPC）。
    /// <para>
    /// 订阅 <see cref="NotificationPushHub"/>，将实时通知事件以 JSON 载荷推送至客户端。
    /// 流持续到客户端断开、取消或 gRPC 截止时间到达。
    /// </para>
    /// </summary>
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

    /// <summary>
    /// 订阅流量数据事件流（服务端流式 RPC）。
    /// <para>
    /// 订阅 <see cref="TrafficPushHub"/>，将实时流量事件以 JSON 载荷推送至客户端。
    /// 流持续到客户端断开、取消或 gRPC 截止时间到达。
    /// </para>
    /// </summary>
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
