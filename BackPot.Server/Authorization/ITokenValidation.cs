namespace BackPot.Server.Authorization;

public interface ITokenValidation
{
    bool IsValidToken(string token);
}
