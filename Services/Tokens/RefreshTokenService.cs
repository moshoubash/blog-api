using DotnetAPI.Models;
using DotnetAPI.Dtos.Authentication.RefreshToken;
using System.Security.Cryptography;
using DotnetAPI.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetAPI.Services.Tokens;

public class RefreshTokenService : ITokenService
{
    private readonly int _refreshTokenExpiryDays;
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    
    public RefreshTokenService(IOptions<ApiSettings> configOptions, ApplicationDbContext context, JwtTokenService jwtTokenService)
    {
        _refreshTokenExpiryDays = configOptions.Value.RefreshTokenExpiryDays > 0 ? configOptions.Value.RefreshTokenExpiryDays : 15;
        _context = context;
        _jwtTokenService = jwtTokenService;
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

    public async Task<TokenRefreshResult> RefreshAccessTokenAsync(string refreshToken)
    {
        // Check if refresh token exists and is valid
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.AppUser!)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (storedToken?.AppUser is null)
        {
            return new TokenRefreshResult { Success = false, Error = "Invalid refresh token" };
        }

        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            // Token has expired, mark it as revoked
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();
            return new TokenRefreshResult { Success = false, Error = "Refresh token expired" };
        }

        // Generate new access token
        var (newAccessToken, _) = _jwtTokenService.GenerateToken(storedToken.AppUser);

        // Rotate refresh token
        var (newRefreshToken, newRefreshTokenExpiry) = GenerateToken(storedToken.AppUser);

        return new TokenRefreshResult
        {
            Success = true,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiry = newRefreshTokenExpiry
        };
    }

    public async Task RevokeRefreshTokenAsync(int userId, string? refreshToken = null)
    {
        // If refresh token is provided, revoke only that token
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
            return;
        }

        // Otherwise revoke all refresh tokens for the user
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();
    }
}