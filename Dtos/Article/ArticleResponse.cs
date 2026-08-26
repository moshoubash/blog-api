namespace DotnetAPI.Dtos.Article;

public sealed record ArticleResponse(
    int Id,
    string Title,
    string? Slug,
    string Content,
    string? Excerpt,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    bool IsPublished,
    int AuthorId,
    string AuthorName,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Tags);
