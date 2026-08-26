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
    [Authorize(Roles = "Author,Reader")]
    public Task<PagedResponse<ArticleResponse>> GetArticles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return articleRepository.GetArticlesAsync(page, pageSize, cancellationToken);
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
        var article = await articleRepository.CreateArticleAsync(request, GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
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
        var article = await articleRepository.UpdateArticleAsync(id, request, GetUserId(), cancellationToken);
        return article is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "Article not found or is not owned by the current user.")
            : Ok(article);
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
