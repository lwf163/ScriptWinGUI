using System.Security.Cryptography;
using System.Text;
using SwgServer;
using Xunit;

namespace Swg.Grpc.Tests;

public class TokenValidatorTests
{
    private static (string hmacSecret, string signedToken, string plainToken) GenerateTestToken()
    {
        var hmacKey = RandomNumberGenerator.GetBytes(32);
        var hmacSecret = Convert.ToBase64String(hmacKey);
        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        using var hmac = new HMACSHA256(hmacKey);
        var signedToken = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(plainToken)));
        return (hmacSecret, signedToken, plainToken);
    }

    [Fact]
    public void Validate_CorrectToken_ReturnsTrue()
    {
        var (secret, signed, plain) = GenerateTestToken();
        var validator = new TokenValidator(secret, signed);
        Assert.True(validator.Validate(plain));
    }

    [Fact]
    public void Validate_WrongToken_ReturnsFalse()
    {
        var (secret, signed, _) = GenerateTestToken();
        var validator = new TokenValidator(secret, signed);
        Assert.False(validator.Validate("wrong-token"));
    }

    [Fact]
    public void Validate_NullToken_ReturnsFalse()
    {
        var (secret, signed, _) = GenerateTestToken();
        var validator = new TokenValidator(secret, signed);
        Assert.False(validator.Validate(null));
    }

    [Fact]
    public void Validate_EmptyToken_ReturnsFalse()
    {
        var (secret, signed, _) = GenerateTestToken();
        var validator = new TokenValidator(secret, signed);
        Assert.False(validator.Validate(""));
    }

    [Fact]
    public void Validate_FromConfig_WorksEndToEnd()
    {
        var (secret, signed, plain) = GenerateTestToken();
        var config = new SwgServerConfig.AuthConfig.TokenConfig
        {
            HmacSecret = secret,
            SignedToken = signed,
        };
        var validator = new TokenValidator(config);
        Assert.True(validator.Validate(plain));
    }
}
