using DotnetAPI.Database;
using DotnetAPI.Dtos.Authentication;
using DotnetAPI.Models;
using DotnetAPI.Services;
using DotnetAPI.Services.Tokens;
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
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var tokenService = tokenFactory.Create("jwt");
        var token = tokenService.GenerateToken(user);

        return Ok(CreateResponse(user, token));
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        if (await dbContext.Users.AnyAsync(user => user.Username == username, cancellationToken))
        {
            return Conflict(new { message = "Username is already in use." });
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Username = username,
            RoleId = 1
        };
        user.Password = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(user).Reference(item => item.Role).LoadAsync(cancellationToken);

        var token = tokenFactory.Create("jwt").GenerateToken(user);
        return Created("api/auth/me", CreateResponse(user, token));
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

    private static AuthResponse CreateResponse(User user, (string Token, DateTime ExpiresAt) token) =>
        new(token.Token, token.ExpiresAt, new UserResponse(
            user.Id,
            user.Name,
            user.Username,
            user.Role?.Name ?? string.Empty,
            user.CreatedAt));
}
