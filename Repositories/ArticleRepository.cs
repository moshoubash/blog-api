using System.Text.Json;
using System.Text.Json.Nodes;
using DotnetAPI.Database;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Repositories;

public class ArticleRepository : IArticleRepository
{
    protected readonly ApplicationDbContext _dbContext;

    public ArticleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Article GetArticle(int id)
    {
        var targetarticle = _dbContext.Articles.FirstOrDefault(a => a.Id == id);
        if (targetarticle == null)
            throw new Exception("Article not found");
        
        return targetarticle;
    }

    public Task<IEnumerable<Article>> GetArticles(int pageNumber, int pageSize)
    {
        try
        {
            if (!_dbContext.Articles.Any())
                throw new Exception("No articles found");

            return Task.FromResult<IEnumerable<Article>>(_dbContext.Articles
                .AsNoTracking()
                .Where(a => a.CreatedAt.Date >= DateTime.Now.Date.AddDays(-7)) // articles older than 7 days
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize));
        }
        catch (Exception exception)
        {
            return Task.FromException<IEnumerable<Article>>(exception);
        }
    }

    public async Task<object> CreateArticle(Dtos.Article.CreateArticle createArticle)
    {
        try
        {
            await _dbContext.Articles.AddAsync(new Article
            {
                Title = createArticle.Title,
                CreatedAt = DateTime.Now,
                UserId = 1
            });

            await _dbContext.SaveChangesAsync();
            return new { message = "article was added" };
        }
        catch (Exception e)
        {
            return new { error = e.Message };
        }
    }

    public async Task<object> UpdateArticle(Dtos.Article.EditArticle article)
    {
        try
        {
            var targetarticle = await _dbContext.Articles.FirstOrDefaultAsync(a => a.Id == article.Id);
            
            if (targetarticle == null)
            {
                return new { error = "Article not found" };
            }

            targetarticle.Title = article.Title;

            _dbContext.Articles.Update(targetarticle);
            await _dbContext.SaveChangesAsync();

            return new
            {
                message = targetarticle
            };
        }
        catch (Exception e)
        {
            return new { error = e.Message };
        }
    }

    public Task<object> DeleteArticle(Dtos.Article.DeleteArticle deleteArticle)
    {
        var targetArticle = _dbContext.Articles.FirstOrDefault(a => a.Id == deleteArticle.Id);
        
        if(targetArticle == null)
            throw new Exception("Article not found");       
        
        _dbContext.Articles.Remove(targetArticle);
        _dbContext.SaveChanges();

       return Task.FromResult<object>(new { message = "Article was deleted" });
    }
}