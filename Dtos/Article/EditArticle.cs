using System.ComponentModel.DataAnnotations;

namespace DotnetAPI.Dtos.Article;

public sealed class EditArticle
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Title { get; init; } = string.Empty;
}
