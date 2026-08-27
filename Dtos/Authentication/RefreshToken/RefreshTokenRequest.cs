namespace DotnetAPI.Dtos.Authentication.RefreshToken;

public class RefreshTokenRequest(string refreshToken)
{
    public string RefreshToken { get; } = refreshToken;
}