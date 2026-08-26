using DotnetAPI.Database;
using DotnetAPI.Dtos.Article;
using DotnetAPI.Dtos.Common;
using DotnetAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Repositories;

public sealed class ArticleRepository(ApplicationDbContext dbContext) : IArticleRepository
{
    public Task<ArticleResponse?> GetArticleAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Articles
            .AsNoTracking()
            .Where(article => article.Id == id)
            .Select(article => new ArticleResponse(
                article.Id,
                article.Title,
                article.CreatedAt,
                article.UserId,
                article.User!.Name))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<ArticleResponse>> GetArticlesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Articles.AsNoTracking();
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(article => article.CreatedAt)
            .ThenByDescending(article => article.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(article => new ArticleResponse(
                article.Id,
                article.Title,
                article.CreatedAt,
                article.UserId,
                article.User!.Name))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new PagedResponse<ArticleResponse>(items, page, pageSize, totalItems, totalPages);
    }

    public async Task<ArticleResponse> CreateArticleAsync(
        CreateArticle request,
        int userId,
        CancellationToken cancellationToken)
    {
        var article = new Article
        {
            Title = request.Title.Trim(),
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync(cancellationToken);

        var authorName = await dbContext.Users
            .Where(user => user.Id == userId)
            .Select(user => user.Name)
            .SingleAsync(cancellationToken);

        return new ArticleResponse(article.Id, article.Title, article.CreatedAt, userId, authorName);
    }

    public async Task<ArticleResponse?> UpdateArticleAsync(
        int id,
        EditArticle request,
        int userId,
        CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles
            .Include(item => item.User)
            .SingleOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (article is null)
        {
            return null;
        }

        article.Title = request.Title.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ArticleResponse(article.Id, article.Title, article.CreatedAt, article.UserId, article.User!.Name);
    }

    public async Task<bool> DeleteArticleAsync(int id, int userId, CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles
            .SingleOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (article is null)
        {
            return false;
        }

        dbContext.Articles.Remove(article);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
