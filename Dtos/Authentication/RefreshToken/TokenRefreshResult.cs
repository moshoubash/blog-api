namespace DotnetAPI.Dtos.Authentication.RefreshToken;

public class TokenRefreshResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}
