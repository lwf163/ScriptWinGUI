using System.Security.Cryptography;
using System.Text;
using SwgServer;

internal sealed class TokenValidator
{
    private readonly string _hmacSecretBase64;
    private readonly byte[] _expectedSignedTokenBytes;

    public TokenValidator(SwgServerConfig.AuthConfig.TokenConfig tokenConfig)
        : this(tokenConfig.HmacSecret, tokenConfig.SignedToken) { }

    public TokenValidator(string hmacSecretBase64, string expectedSignedToken)
    {
        _hmacSecretBase64 = hmacSecretBase64;
        _expectedSignedTokenBytes = Encoding.UTF8.GetBytes(expectedSignedToken);
    }

    public bool Validate(string? plainToken)
    {
        if (string.IsNullOrEmpty(plainToken))
            return false;

        var hmacKey = Convert.FromBase64String(_hmacSecretBase64);
        using var hmac = new HMACSHA256(hmacKey);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainToken));
        var computed = Convert.ToBase64String(hashBytes);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            _expectedSignedTokenBytes);
    }
}
