using DotnetAPI.Dtos.Article;
using DotnetAPI.Dtos.Common;

namespace DotnetAPI.Repositories;

public interface IArticleRepository
{
    Task<ArticleResponse?> GetArticleAsync(int id, CancellationToken cancellationToken);
    Task<PagedResponse<ArticleResponse>> GetArticlesAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<ArticleResponse> CreateArticleAsync(CreateArticle request, int userId, CancellationToken cancellationToken);
    Task<ArticleResponse?> UpdateArticleAsync(int id, EditArticle request, int userId, CancellationToken cancellationToken);
    Task<bool> DeleteArticleAsync(int id, int userId, CancellationToken cancellationToken);
}
