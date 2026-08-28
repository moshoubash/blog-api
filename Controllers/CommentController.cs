using System.Security.Claims;
using DotnetAPI.Database;
using DotnetAPI.Dtos.Blog;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("api/articles/{articleId:int}/comments")]
public sealed class CommentController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetComments(int articleId, CancellationToken cancellationToken)
    {
        var comments = await dbContext.Comments.AsNoTracking()
            .Where(comment => comment.ArticleId == articleId && comment.IsApproved)
            .OrderBy(comment => comment.CreatedAt)
            .Select(comment => new CommentResponse(comment.Id, comment.Content, comment.CreatedAt, comment.UserId, comment.User.Name))
            .ToListAsync(cancellationToken);
        return Ok(comments);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> CreateComment(int articleId, CommentRequest request, CancellationToken cancellationToken)
    {
        var articleExists = await dbContext.Articles.AnyAsync(article => article.Id == articleId && article.IsPublished, cancellationToken);
        if (!articleExists)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Article not found.");
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var comment = new Comment { ArticleId = articleId, UserId = userId, Content = request.Content.Trim() };
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);
        var userName = await dbContext.Users.Where(user => user.Id == userId).Select(user => user.Name).SingleAsync(cancellationToken);
        return CreatedAtAction(nameof(GetComments), new { articleId }, new CommentResponse(comment.Id, comment.Content, comment.CreatedAt, userId, userName));
    }

    [HttpDelete("{commentId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int articleId, int commentId, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var comment = await dbContext.Comments.SingleOrDefaultAsync(item => item.Id == commentId && item.ArticleId == articleId && item.UserId == userId, cancellationToken);
        if (comment is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Comment not found or is not owned by the current user.");
        }
        dbContext.Comments.Remove(comment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
