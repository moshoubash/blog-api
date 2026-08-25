using DotnetAPI.Database;
using DotnetAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Dtos.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(JwtTokenService jwtTokenService, ApplicationDbContext dbContext)
    : ControllerBase
{
    [HttpPost("login")]
    public ActionResult Login([FromBody] LoginRequest request)
    {
        var user = dbContext.Users
            .Include(u => u.Role)
            .SingleOrDefault(u => 
            u.Username == request.Username && 
            u.Password == request.Password);

        if (user == null)
            return Unauthorized("Invalid credentials");

        var token = jwtTokenService.GenerateToken(user);

        return Ok(new { token });
    }
}