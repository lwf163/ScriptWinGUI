using Grpc.Core;
using Grpc.Core.Interceptors;
using Swg.Grpc.Capture;
using Swg.Grpc.Cv;
using Swg.Grpc.Flaui;
using Swg.Grpc.Fs;
using Swg.Grpc.Input;
using Swg.Grpc.Ocr;
using Swg.Grpc.Services;
using Swg.Grpc.Win32;

namespace Swg.Grpc;

/// <summary>
/// 将各 <see cref="ServerServiceDefinition"/> 绑定到 <see cref="Server"/> 时使用的工厂（宿主进程侧使用；库内仅提供定义）。
/// </summary>
public static class SwgGrpcServiceBinder
{
    /// <summary>
    /// 返回当前库内已实现的全部 gRPC 服务定义，可选按顺序注入多个全局拦截器。
    /// </summary>
    /// <remarks>
    /// 绑定顺序：对数组依次执行 <c>definition.Intercept(i)</c>，后绑定的拦截器位于更外层（先入站）。
    /// 典型顺序：<c>[认证, 期限]</c> → 外层为「期限」，对整个管道（含认证与业务）计时。
    /// </remarks>
    public static IEnumerable<ServerServiceDefinition> GetServiceDefinitions(params Interceptor[] interceptors)
    {
        yield return Bind(Win32Service.BindService(new Win32GrpcService()), interceptors);
        yield return Bind(CvService.BindService(new CvGrpcService()), interceptors);
        yield return Bind(InputService.BindService(new InputGrpcService()), interceptors);
        yield return Bind(OcrService.BindService(new OcrGrpcService()), interceptors);
        yield return Bind(FsService.BindService(new FsGrpcService()), interceptors);
        yield return Bind(CaptureService.BindService(new CaptureGrpcService()), interceptors);
        yield return Bind(AutomationService.BindService(new FlaUIGrpcService()), interceptors);
    }

    private static ServerServiceDefinition Bind(ServerServiceDefinition definition, Interceptor[]? interceptors)
    {
        if (interceptors is null || interceptors.Length == 0)
            return definition;

        foreach (var interceptor in interceptors)
            definition = definition.Intercept(interceptor);

        return definition;
    }
}
