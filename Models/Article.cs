using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetAPI.Models;

public class Article
{
    [Key]
    [DatabaseGenerated( DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Excerpt { get; set; }
    
    [MaxLength(300)]
    public string? Slug { get; private set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAt { get; set; }

    public bool IsPublished { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }

    public User? User { get; set; }

    public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();

    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
