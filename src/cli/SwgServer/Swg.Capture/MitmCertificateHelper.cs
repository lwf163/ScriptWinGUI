using Titanium.Web.Proxy;

namespace Swg.Capture;

/// <summary>
/// 将 MITM 证书选项应用到代理服务器的证书管理器（幂等配置，具体生成由库完成）。
/// </summary>
public static class MitmCertificateHelper
{
    public static void Apply(ProxyServer server, MitmCertificateOptions options)
    {
        ArgumentNullException.ThrowIfNull(server);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.UserTrustRoot && !options.MachineTrustRoot)
            return;

        bool alsoMachine = options.MachineTrustRoot;
        if (options.TrustRootAsAdministrator)
        {
            _ = server.CertificateManager.TrustRootCertificateAsAdmin(alsoMachine);
        }
        else
        {
            server.CertificateManager.TrustRootCertificate(alsoMachine);
        }
    }
}
