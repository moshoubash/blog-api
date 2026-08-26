using System.ComponentModel.DataAnnotations;

namespace DotnetAPI.Dtos.Article;

public sealed class CreateArticle
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Title { get; init; } = string.Empty;
}
