namespace DotnetAPI.Dtos.Article;

public sealed record ArticleResponse(
    int Id,
    string Title,
    DateTime CreatedAt,
    int AuthorId,
    string AuthorName);
