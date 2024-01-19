using Microsoft.Extensions.Options;

namespace BackPot.Server.Authorization;

internal class TokenValidation(IOptions<BackPotServerOptions> options) : ITokenValidation
{
    private readonly BackPotServerOptions _options = options.Value;

    public bool IsValidToken(string token)
        => token == _options.Token;
}
