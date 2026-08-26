using System.ComponentModel.DataAnnotations;

namespace DotnetAPI.Dtos.Article;

public sealed class EditArticle
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Title { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string Content { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Excerpt { get; init; }

    public bool IsPublished { get; init; }
    public IReadOnlyList<int> CategoryIds { get; init; } = [];
    public IReadOnlyList<int> TagIds { get; init; } = [];
}
