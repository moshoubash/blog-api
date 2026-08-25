using DotnetAPI.Models;
using DotnetAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class ArticleController(IArticleRepository articleRepository)
{
    [HttpGet("{id:int}")]
    public Article GetArticle(int id)
    {
        return articleRepository.GetArticle(id);
    }
    
    [HttpGet("page/{pageNumber:int}/{pageSize:int}")]
    [Route("articles")]
    [Authorize(Roles = "Author")]
    public async Task<IEnumerable<Article>> GetArticles(int pageNumber, int pageSize)
    {
        return await articleRepository.GetArticles(pageNumber, pageSize);
    }
    
    [HttpPost]
    public async Task<object> CreateArticle(Dtos.Article.CreateArticle createArticle)
    {
        return await articleRepository.CreateArticle(createArticle);
    }
    
    [HttpPut]
    public async Task<object> UpdateArticle(Dtos.Article.EditArticle article)
    {
        return await articleRepository.UpdateArticle(article);
    }
    
    [HttpDelete]
    public async Task<object> DeleteArticle(Dtos.Article.DeleteArticle deleteArticle)
    {
        return await articleRepository.DeleteArticle(deleteArticle);
    }
}