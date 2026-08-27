namespace DotnetAPI.Services.Tokens;

using DotnetAPI.Models;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}