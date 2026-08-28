using DotnetAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasMany(u => u.Articles)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId);

        builder.Entity<Article>()
            .HasOne(a => a.User)
            .WithMany(u => u.Articles);
        
        builder.Entity<Role>()
            .HasMany(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId);
        
        builder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users);
        
        // Generating a slug for the article title
        builder.Entity<Article>()
            .Property(a => a.Slug)
            .HasComputedColumnSql("LOWER(REPLACE(Title, ' ', '-'))", stored: true);

        builder.Entity<ArticleCategory>().HasKey(item => new { item.ArticleId, item.CategoryId });
        builder.Entity<ArticleCategory>()
            .HasOne(item => item.Article).WithMany(item => item.ArticleCategories).HasForeignKey(item => item.ArticleId);
        builder.Entity<ArticleCategory>()
            .HasOne(item => item.Category).WithMany(item => item.ArticleCategories).HasForeignKey(item => item.CategoryId);

        builder.Entity<ArticleTag>().HasKey(item => new { item.ArticleId, item.TagId });
        builder.Entity<ArticleTag>()
            .HasOne(item => item.Article).WithMany(item => item.ArticleTags).HasForeignKey(item => item.ArticleId);
        builder.Entity<ArticleTag>()
            .HasOne(item => item.Tag).WithMany(item => item.ArticleTags).HasForeignKey(item => item.TagId);

        builder.Entity<Comment>()
            .HasOne(item => item.Article).WithMany(item => item.Comments).HasForeignKey(item => item.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Comment>()
            .HasOne(item => item.User).WithMany(item => item.Comments).HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Category>().HasIndex(item => item.Slug).IsUnique();
        builder.Entity<Tag>().HasIndex(item => item.Slug).IsUnique();
        
        // SEEDING
        builder.Entity<Role>().HasData(
            new Role { Id = Role.AuthorId, Name = Role.Author }
        );
        
        builder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Author",
                Username = "author",
                Password = "12341234",
                RoleId = Role.AuthorId,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        builder.Entity<Article>().HasData(
            new Article { Id = 1, Title = "First Article", Content = "Welcome to the first article.", UserId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsPublished = true },
            new Article { Id = 2, Title = "Second Article", Content = "This is the second article.", UserId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsPublished = true },
            new Article { Id = 3, Title = "Third Article", Content = "This is the third article.", UserId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), PublishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsPublished = true }
        );
    }

    public DbSet<Article> Articles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ArticleCategory> ArticleCategories { get; set; }
    public DbSet<ArticleTag> ArticleTags { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
