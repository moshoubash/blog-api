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
            .Where(article => article.Id == id && article.IsPublished)
            .Select(article => new ArticleResponse(
                article.Id,
                article.Title,
                article.Slug,
                article.Content,
                article.Excerpt,
                article.CreatedAt,
                article.PublishedAt,
                article.IsPublished,
                article.UserId,
                article.User!.Name,
                article.ArticleCategories.Select(item => item.Category.Name).ToList(),
                article.ArticleTags.Select(item => item.Tag.Name).ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<ArticleResponse?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        return Project(dbContext.Articles.AsNoTracking().Where(article => article.Slug == slug && article.IsPublished))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<ArticleResponse>> GetArticlesAsync(
        int page,
        int pageSize,
        string? search,
        string? category,
        string? tag,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Articles.AsNoTracking().Where(article => article.IsPublished);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(article => article.Title.Contains(term) || article.Content.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(article => article.ArticleCategories.Any(item => item.Category.Slug == category));
        }
        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(article => article.ArticleTags.Any(item => item.Tag.Slug == tag));
        }
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await Project(query
            .OrderByDescending(article => article.CreatedAt)
            .ThenByDescending(article => article.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize))
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
            Content = request.Content.Trim(),
            Excerpt = request.Excerpt?.Trim(),
            CreatedAt = DateTime.UtcNow,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            IsPublished = request.IsPublished,
            UserId = userId,
            ArticleCategories = await GetCategoriesAsync(request.CategoryIds, cancellationToken),
            ArticleTags = await GetTagsAsync(request.TagIds, cancellationToken)
        };

        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync(cancellationToken);

        var authorName = await dbContext.Users
            .Where(user => user.Id == userId)
            .Select(user => user.Name)
            .SingleAsync(cancellationToken);

        return await GetArticleForAuthorAsync(article.Id, cancellationToken) ?? throw new InvalidOperationException("Article could not be loaded after creation.");
    }

    public async Task<ArticleResponse?> UpdateArticleAsync(
        int id,
        EditArticle request,
        int userId,
        CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles
            .Include(item => item.User)
            .Include(item => item.ArticleCategories)
            .Include(item => item.ArticleTags)
            .SingleOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (article is null)
        {
            return null;
        }

        article.Title = request.Title.Trim();
        article.Content = request.Content.Trim();
        article.Excerpt = request.Excerpt?.Trim();
        article.IsPublished = request.IsPublished;
        article.PublishedAt ??= request.IsPublished ? DateTime.UtcNow : null;
        article.ArticleCategories.Clear();
        article.ArticleTags.Clear();
        article.ArticleCategories = await GetCategoriesAsync(request.CategoryIds, cancellationToken);
        article.ArticleTags = await GetTagsAsync(request.TagIds, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetArticleForAuthorAsync(article.Id, cancellationToken);
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

    private async Task<List<ArticleCategory>> GetCategoriesAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken)
    {
        var distinctIds = ids.Distinct().ToArray();
        var categories = await dbContext.Categories.Where(category => distinctIds.Contains(category.Id)).ToListAsync(cancellationToken);
        if (categories.Count != distinctIds.Length)
        {
            throw new ArgumentException("One or more category IDs are invalid.");
        }
        return categories.Select(category => new ArticleCategory { CategoryId = category.Id, Category = category }).ToList();
    }

    private async Task<List<ArticleTag>> GetTagsAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken)
    {
        var distinctIds = ids.Distinct().ToArray();
        var tags = await dbContext.Tags.Where(tag => distinctIds.Contains(tag.Id)).ToListAsync(cancellationToken);
        if (tags.Count != distinctIds.Length)
        {
            throw new ArgumentException("One or more tag IDs are invalid.");
        }
        return tags.Select(tag => new ArticleTag { TagId = tag.Id, Tag = tag }).ToList();
    }

    private Task<ArticleResponse?> GetArticleForAuthorAsync(int id, CancellationToken cancellationToken) =>
        Project(dbContext.Articles.AsNoTracking().Where(article => article.Id == id)).SingleOrDefaultAsync(cancellationToken);

    private static IQueryable<ArticleResponse> Project(IQueryable<Article> query) => query.Select(article => new ArticleResponse(
        article.Id,
        article.Title,
        article.Slug,
        article.Content,
        article.Excerpt,
        article.CreatedAt,
        article.PublishedAt,
        article.IsPublished,
        article.UserId,
        article.User!.Name,
        article.ArticleCategories.Select(item => item.Category.Name).ToList(),
        article.ArticleTags.Select(item => item.Tag.Name).ToList()));
}
