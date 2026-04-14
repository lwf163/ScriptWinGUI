namespace Swg.Capture;

/// <summary>
/// HTTPS 解密（MITM）根证书信任范围，映射到 <c>Titanium.Web.Proxy</c> 的 <c>CertificateManager</c>。
/// </summary>
public sealed class MitmCertificateOptions
{
    /// <summary>是否将根证书安装到当前用户「受信任的根证书颁发机构」。</summary>
    public bool UserTrustRoot { get; set; } = true;

    /// <summary>是否安装到本机存储（常需管理员）。</summary>
    public bool MachineTrustRoot { get; set; }

    /// <summary>以管理员身份信任根证书（与 MachineTrustRoot 等组合使用）。</summary>
    public bool TrustRootAsAdministrator { get; set; }
}
