using System.ComponentModel.DataAnnotations;

namespace DotnetAPI.Models;

public sealed class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Slug { get; set; } = string.Empty;

    public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();
}
