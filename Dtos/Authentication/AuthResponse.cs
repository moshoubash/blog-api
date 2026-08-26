namespace DotnetAPI.Dtos.Authentication;

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserResponse User);
