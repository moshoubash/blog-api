namespace DotnetAPI.Dtos.Article;

public sealed record ArticleResponse(
    int Id,
    string Title,
    string Slug,
    DateTime CreatedAt,
    int AuthorId,
    string AuthorName);
