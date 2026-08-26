using System.ComponentModel.DataAnnotations;

namespace DotnetAPI.Dtos.Blog;

public sealed class TaxonomyRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
}

public sealed class CommentRequest
{
    [Required, StringLength(2000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;
}

public sealed record TaxonomyResponse(int Id, string Name, string Slug);
public sealed record CommentResponse(int Id, string Content, DateTime CreatedAt, int UserId, string UserName);
