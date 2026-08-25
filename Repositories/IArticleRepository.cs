using DotnetAPI.Models;

namespace DotnetAPI.Repositories;

public interface IArticleRepository
{
    public Article GetArticle(int id);
    
    public Task<IEnumerable<Article>> GetArticles(int pageNumber, int pageSize);
    
    public Task<object> CreateArticle(Dtos.Article.CreateArticle createArticle);
    
    public Task<object> UpdateArticle(Dtos.Article.EditArticle article);
    
    public Task<object> DeleteArticle(Dtos.Article.DeleteArticle deleteArticle);
}