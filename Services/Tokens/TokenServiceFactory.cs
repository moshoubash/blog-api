using DotnetAPI.Dtos.Authentication.RefreshToken;

namespace DotnetAPI.Services.Tokens;

public sealed class TokenServiceFactory(
    JwtTokenService jwtTokenService,
    RefreshTokenService refreshTokenService)
{
    public JwtTokenService Jwt => jwtTokenService;
    public RefreshTokenService RefreshToken => refreshTokenService;

    public ITokenService Create(string tokenType)
    {
        return tokenType.ToLowerInvariant() switch
        {
            "jwt" => jwtTokenService,
            "refreshtoken" => refreshTokenService,
            _ => throw new ArgumentException($"Unsupported token type: {tokenType}")
        };
    }

    public Task<TokenRefreshResult> RefreshAccessTokenAsync(string refreshToken) =>
        refreshTokenService.RefreshAccessTokenAsync(refreshToken);

    public Task RevokeRefreshTokenAsync(int userId, string? refreshToken = null) =>
        refreshTokenService.RevokeRefreshTokenAsync(userId, refreshToken);
}