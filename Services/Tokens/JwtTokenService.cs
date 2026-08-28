using Microsoft.EntityFrameworkCore;
using DotnetAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using DotnetAPI.Dtos.Authentication.RefreshToken;
using Microsoft.Extensions.Options;

namespace DotnetAPI.Services.Tokens;

public class JwtTokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;

    public JwtTokenService(IOptions<ApiSettings> configOptions)
    {
        _secretKey = configOptions.Value.Secret ?? throw new InvalidOperationException("ApiSettings:Secret is required.");
        _issuer = configOptions.Value.Issuer ?? throw new InvalidOperationException("ApiSettings:Issuer is required.");
        _audience = configOptions.Value.Audience ?? throw new InvalidOperationException("ApiSettings:Audience is required.");
        _accessTokenExpiryMinutes = configOptions.Value.AccessTokenExpiryMinutes > 0 ? configOptions.Value.AccessTokenExpiryMinutes : 60;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
