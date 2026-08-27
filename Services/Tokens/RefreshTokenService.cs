using DotnetAPI.Models;
using DotnetAPI.Dtos.Authentication.RefreshToken;
using System.Security.Cryptography;
using DotnetAPI.Database;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Services.Tokens;

public class RefreshTokenService : ITokenService
{
    private readonly int _refreshTokenExpiryDays;
    private readonly ApplicationDbContext _context;
    
    public RefreshTokenService(IConfiguration config, ApplicationDbContext context)
    {
        _refreshTokenExpiryDays = config.GetValue<int>("ApiSettings:RefreshTokenExpiryDays", 7);
        _context = context;
    }

    public void StoreTheRefreshToken(int userId, RefreshToken refreshToken)
    {
        var existingTokens = _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToList();
        
        foreach (var token in existingTokens)
        {
            token.IsRevoked = true;
        }

        // Add the new token to db
        _context.RefreshTokens.Add(refreshToken);
        _context.SaveChanges();
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            Created = DateTime.UtcNow,
            IsRevoked = false
        };
        
        StoreTheRefreshToken(refreshToken.UserId, refreshToken);
        
        return (refreshToken.Token, refreshToken.ExpiryDate);
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}