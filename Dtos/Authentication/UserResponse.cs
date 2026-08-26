namespace DotnetAPI.Dtos.Authentication;

public sealed record UserResponse(
    int Id,
    string Name,
    string Username,
    string Role,
    DateTime CreatedAt);
