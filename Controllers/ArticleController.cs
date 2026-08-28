using System.Security.Claims;
using DotnetAPI.Dtos.Article;
using DotnetAPI.Dtos.Common;
using DotnetAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("api/articles")]
[EnableRateLimiting("api")]
public sealed class ArticleController(IArticleRepository articleRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<ArticleResponse>>(StatusCodes.Status200OK)]
    public Task<PagedResponse<ArticleResponse>> GetArticles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? tag = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return articleRepository.GetArticlesAsync(page, pageSize, search, category, tag, cancellationToken);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ArticleResponse>> GetArticleBySlug(string slug, CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetArticleBySlugAsync(slug, cancellationToken);
        return article is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "Article not found.")
            : Ok(article);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleResponse>> GetArticle(int id, CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetArticleAsync(id, cancellationToken);
        return article is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "Article not found.")
            : Ok(article);
    }

    [HttpPost]
    [Authorize(Roles = "Author")]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ArticleResponse>> CreateArticle(
        CreateArticle request,
        CancellationToken cancellationToken)
    {
        try
        {
            var article = await articleRepository.CreateArticleAsync(request, GetUserId(), cancellationToken);
            return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: exception.Message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Author")]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleResponse>> UpdateArticle(
        int id,
        EditArticle request,
        CancellationToken cancellationToken)
    {
        try
        {
            var article = await articleRepository.UpdateArticleAsync(id, request, GetUserId(), cancellationToken);
            return article is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "Article not found or is not owned by the current user.")
                : Ok(article);
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: exception.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Author")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteArticle(int id, CancellationToken cancellationToken)
    {
        var deleted = await articleRepository.DeleteArticleAsync(id, GetUserId(), cancellationToken);
        return deleted
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status404NotFound, detail: "Article not found or is not owned by the current user.");
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
