using System.Security.Claims;
using DotnetAPI.Database;
using DotnetAPI.Dtos.Authentication;
using DotnetAPI.Dtos.Authentication.RefreshToken;
using DotnetAPI.Models;
using DotnetAPI.Services;
using DotnetAPI.Services.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("login")]
public sealed class AuthController(
    TokenServiceFactory tokenFactory,
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(item => item.Role)
            .SingleOrDefaultAsync(item => item.Username == request.Username.Trim(), cancellationToken);

        if (user is null || !VerifyPassword(user, request.Password))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Invalid username or password.");
        }

        var jwttokenService = tokenFactory.Create("jwt");
        var jwtToken = jwttokenService.GenerateToken(user);
        
        var refreshtokenService = tokenFactory.Create("refreshtoken");
        var refreshToken = refreshtokenService.GenerateToken(user);
        
        return Ok(CreateResponse(user, jwtToken, refreshToken));
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        if (await dbContext.Users.AnyAsync(user => user.Username == username, cancellationToken))
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Username is already in use.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Username = username,
            RoleId = Role.AuthorId
        };
        user.Password = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(user).Reference(item => item.Role).LoadAsync(cancellationToken);

        var jwttokenService = tokenFactory.Create("jwt");
        var jwtToken = jwttokenService.GenerateToken(user);
        
        var refreshtokenService = tokenFactory.Create("refreshtoken");
        var refreshToken = refreshtokenService.GenerateToken(user);

        return Created("api/auth/me", CreateResponse(user, jwtToken, refreshToken));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Refresh token is required.");
        }

        var result = await tokenFactory.RefreshAccessTokenAsync(request.RefreshToken);

        if (!result.Success)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: result.Error ?? "Invalid refresh token.");
        }

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            expiresAt = result.RefreshTokenExpiry
        });
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest? request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Unauthorized.");
        }

        await tokenFactory.RevokeRefreshTokenAsync(userId, request?.RefreshToken);

        return Ok(new { message = "Token revoked successfully" });
    }

    private bool VerifyPassword(User user, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);
        if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            return true;
        }

        // Upgrade the original local seed account on its first successful login.
        if (user.Password == password)
        {
            user.Password = passwordHasher.HashPassword(user, password);
            dbContext.SaveChanges();
            return true;
        }

        return false;
    }

    private static AuthResponse CreateResponse(User user, (string Token, DateTime ExpiresAt) jwtToken, (string Token, DateTime ExpiresAt) refreshToken = default) =>
        new AuthResponse(
            user.Id,
            user.Name,
            user.Username,
            user.Role,
            JwtToken: jwtToken.Token,
            JwtTokenExpiresAt: jwtToken.ExpiresAt,
            RefreshToken: refreshToken.Token,
            RefreshTokenExpiresAt: refreshToken.Token is not null ? refreshToken.ExpiresAt : null
        );
}
