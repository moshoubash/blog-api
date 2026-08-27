namespace DotnetAPI.Dtos.Authentication.RefreshToken;

public class RefreshTokenResponse(string refreshToken, DateTime expiryDate)
{
    public string RefreshToken { get; } = refreshToken;
    public DateTime ExpiryDate { get; } = expiryDate;
}