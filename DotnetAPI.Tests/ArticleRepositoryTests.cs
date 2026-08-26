using DotnetAPI.Database;
using DotnetAPI.Dtos.Article;
using DotnetAPI.Models;
using DotnetAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotnetAPI.Tests;

public sealed class ArticleRepositoryTests
{
    [Fact]
    public async Task GetArticlesAsync_ReturnsRequestedPageAndMetadata()
    {
        await using var dbContext = CreateDbContext();
        SeedUsers(dbContext);
        dbContext.Articles.AddRange(
            new Article { Id = 1, Title = "Oldest", Content = "Oldest content", UserId = 1, IsPublished = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Article { Id = 2, Title = "Middle", Content = "Middle content", UserId = 1, IsPublished = true, CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Article { Id = 3, Title = "Newest", Content = "Newest content", UserId = 1, IsPublished = true, CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc) });
        await dbContext.SaveChangesAsync();

        var result = await new ArticleRepository(dbContext).GetArticlesAsync(2, 2, null, null, null, CancellationToken.None);

        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal("Oldest", Assert.Single(result.Items).Title);
    }

    [Fact]
    public async Task CreateArticleAsync_AssignsAuthenticatedAuthor()
    {
        await using var dbContext = CreateDbContext();
        SeedUsers(dbContext);
        await dbContext.SaveChangesAsync();

        var result = await new ArticleRepository(dbContext)
            .CreateArticleAsync(new CreateArticle { Title = "  New article  ", Content = "Article content", IsPublished = true }, 2, CancellationToken.None);

        Assert.Equal("New article", result.Title);
        Assert.Equal(2, result.AuthorId);
        Assert.Equal("Other Author", result.AuthorName);
    }

    [Fact]
    public async Task UpdateAndDelete_RejectArticlesOwnedByAnotherUser()
    {
        await using var dbContext = CreateDbContext();
        SeedUsers(dbContext);
        dbContext.Articles.Add(new Article { Id = 1, Title = "Protected", Content = "Protected content", UserId = 1, IsPublished = true });
        await dbContext.SaveChangesAsync();
        var repository = new ArticleRepository(dbContext);

        var updated = await repository.UpdateArticleAsync(
            1,
            new EditArticle { Title = "Changed", Content = "Changed content" },
            2,
            CancellationToken.None);
        var deleted = await repository.DeleteArticleAsync(1, 2, CancellationToken.None);

        Assert.Null(updated);
        Assert.False(deleted);
        Assert.Equal("Protected", (await dbContext.Articles.SingleAsync()).Title);
    }

    [Fact]
    public async Task GetArticlesAsync_DoesNotReturnDrafts()
    {
        await using var dbContext = CreateDbContext();
        SeedUsers(dbContext);
        dbContext.Articles.AddRange(
            new Article { Id = 1, Title = "Published", Content = "Visible", UserId = 1, IsPublished = true },
            new Article { Id = 2, Title = "Draft", Content = "Hidden", UserId = 1, IsPublished = false });
        await dbContext.SaveChangesAsync();

        var result = await new ArticleRepository(dbContext)
            .GetArticlesAsync(1, 10, null, null, null, CancellationToken.None);

        Assert.Equal("Published", Assert.Single(result.Items).Title);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedUsers(ApplicationDbContext dbContext)
    {
        dbContext.Roles.Add(new Role { Id = 1, Name = "Author" });
        dbContext.Users.AddRange(
            new User { Id = 1, Name = "Author", Username = "author", Password = "password", RoleId = 1 },
            new User { Id = 2, Name = "Other Author", Username = "other", Password = "password", RoleId = 1 });
    }
}
