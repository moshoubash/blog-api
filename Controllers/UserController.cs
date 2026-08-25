using DotnetAPI.Database;
using DotnetAPI.Models;
using DotnetAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class UserController(ApplicationDbContext dbContext)
{
    [HttpGet("{id:int}")]
    public List<Article> GetUserArticles(int id)
    {
        return dbContext.Articles.Where(a => a.UserId == id).ToList();
    }
    
    [HttpPost]
    public object CreateTempUser()
    {
        var user = dbContext.Users.Add(new User { Name = "Test User", RoleId = 1 });
        
        dbContext.SaveChanges();
        
        return new
        {
            message = "User was created",
            user
        };
    }

    [HttpGet("{id:int}/role")]
    public string GetUserRole(int id)
    {
        return dbContext.Users
            .Where(u => u.Id == id)
            .Select(u => u.Role.Name)
            .FirstOrDefault();
    }
}