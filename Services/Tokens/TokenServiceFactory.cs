namespace DotnetAPI.Services.Tokens;

public sealed class TokenServiceFactory(
    JwtTokenService jwtTokenService
    , RefreshTokenService refreshTokenService)
{
    public ITokenService Create(string tokenType)
    {
        return tokenType.ToLowerInvariant() switch
        {
            "jwt" => jwtTokenService,
            "refreshtoken" => refreshTokenService,
            _ => throw new ArgumentException($"Unsupported token type: {tokenType}")
        };
    }
}