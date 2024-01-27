using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Text;

namespace BackPot.Server.Authorization;

internal class TokenValidation(IOptions<BackPotServerOptions> options) : ITokenValidation
{
    private readonly BackPotServerOptions _options = options.Value;

    public bool IsValidToken(string token)
        => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(token), Encoding.UTF8.GetBytes(_options.Token));
}
