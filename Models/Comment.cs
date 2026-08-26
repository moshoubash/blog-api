using System.ComponentModel.DataAnnotations;

namespace DotnetAPI.Models;

public sealed class Comment
{
    public int Id { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsApproved { get; set; } = true;

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
