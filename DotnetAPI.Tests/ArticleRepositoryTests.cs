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
            new Article { Id = 1, Title = "Oldest", UserId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Article { Id = 2, Title = "Middle", UserId = 1, CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Article { Id = 3, Title = "Newest", UserId = 1, CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc) });
        await dbContext.SaveChangesAsync();

        var result = await new ArticleRepository(dbContext).GetArticlesAsync(2, 2, CancellationToken.None);

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
            .CreateArticleAsync(new CreateArticle { Title = "  New article  " }, 2, CancellationToken.None);

        Assert.Equal("New article", result.Title);
        Assert.Equal(2, result.AuthorId);
        Assert.Equal("Other Author", result.AuthorName);
    }

    [Fact]
    public async Task UpdateAndDelete_RejectArticlesOwnedByAnotherUser()
    {
        await using var dbContext = CreateDbContext();
        SeedUsers(dbContext);
        dbContext.Articles.Add(new Article { Id = 1, Title = "Protected", UserId = 1 });
        await dbContext.SaveChangesAsync();
        var repository = new ArticleRepository(dbContext);

        var updated = await repository.UpdateArticleAsync(
            1,
            new EditArticle { Title = "Changed" },
            2,
            CancellationToken.None);
        var deleted = await repository.DeleteArticleAsync(1, 2, CancellationToken.None);

        Assert.Null(updated);
        Assert.False(deleted);
        Assert.Equal("Protected", (await dbContext.Articles.SingleAsync()).Title);
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
