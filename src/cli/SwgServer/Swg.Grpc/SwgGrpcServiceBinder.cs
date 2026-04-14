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
    /// 返回当前库内已实现的全部 gRPC 服务定义，可选注入全局拦截器。
    /// </summary>
    public static IEnumerable<ServerServiceDefinition> GetServiceDefinitions(Interceptor? interceptor = null)
    {
        yield return Bind(Win32Service.BindService(new Win32GrpcService()), interceptor);
        yield return Bind(CvService.BindService(new CvGrpcService()), interceptor);
        yield return Bind(InputService.BindService(new InputGrpcService()), interceptor);
        yield return Bind(OcrService.BindService(new OcrGrpcService()), interceptor);
        yield return Bind(FsService.BindService(new FsGrpcService()), interceptor);
        yield return Bind(CaptureService.BindService(new CaptureGrpcService()), interceptor);
        yield return Bind(AutomationService.BindService(new FlaUIGrpcService()), interceptor);
    }

    private static ServerServiceDefinition Bind(ServerServiceDefinition definition, Interceptor? interceptor) =>
        interceptor is not null ? definition.Intercept(interceptor) : definition;
}
