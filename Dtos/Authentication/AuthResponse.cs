using DotnetAPI.Models;

namespace DotnetAPI.Dtos.Authentication;

public sealed record AuthResponse(
    int Id,
    string Name,
    string Username,
    Role? Role,
    string JwtToken,
    DateTime JwtTokenExpiresAt,
    string? RefreshToken = null,
    DateTime? RefreshTokenExpiresAt = null);