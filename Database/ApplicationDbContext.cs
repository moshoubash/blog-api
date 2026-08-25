using DotnetAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions)
    {
        
    }

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
        
        // SEEDING
        builder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" }, 
            new Role { Id = 2, Name = "User" }
        );
        
        builder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Author",
                RoleId = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        builder.Entity<Article>().HasData(
            new Article { Id = 1, Title = "First Article", UserId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Article { Id = 2, Title = "Second Article", UserId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Article { Id = 3, Title = "Third Article", UserId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }

    public DbSet<Article> Articles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
}