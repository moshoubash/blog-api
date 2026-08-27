namespace DotnetAPI.Services.Tokens;

public sealed class TokenServiceFactory(
    JwtTokenService jwtTokenService)
{
    public ITokenService Create(string tokenType)
    {
        return tokenType.ToLowerInvariant() switch
        {
            "jwt" => jwtTokenService,
            _ => throw new ArgumentException($"Unsupported token type: {tokenType}")
        };
    }
}