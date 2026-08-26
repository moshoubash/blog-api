using System.Security.Claims;
using DotnetAPI.Database;
using DotnetAPI.Dtos.Article;
using DotnetAPI.Dtos.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UserController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(item => item.Role)
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);

        return user is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "User not found.")
            : Ok(new UserResponse(user.Id, user.Name, user.Username, user.Role?.Name ?? string.Empty, user.CreatedAt));
    }

    [HttpGet("me/articles")]
    public async Task<ActionResult<IReadOnlyList<ArticleResponse>>> GetCurrentUserArticles(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var articles = await dbContext.Articles
            .AsNoTracking()
            .Where(article => article.UserId == userId)
            .OrderByDescending(article => article.CreatedAt)
            .Select(article => new ArticleResponse(
                article.Id, article.Title, article.Slug, article.Content, article.Excerpt,
                article.CreatedAt, article.PublishedAt, article.IsPublished, article.UserId,
                article.User!.Name,
                article.ArticleCategories.Select(item => item.Category.Name).ToList(),
                article.ArticleTags.Select(item => item.Tag.Name).ToList()))
            .ToListAsync(cancellationToken);

        return Ok(articles);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
