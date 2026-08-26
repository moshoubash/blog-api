using DotnetAPI.Database;
using DotnetAPI.Dtos.Blog;
using DotnetAPI.Models;
using DotnetAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoryController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaxonomyResponse>>> GetCategories(CancellationToken cancellationToken) =>
        Ok(await dbContext.Categories.AsNoTracking().OrderBy(item => item.Name)
            .Select(item => new TaxonomyResponse(item.Id, item.Name, item.Slug)).ToListAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Author")]
    public async Task<ActionResult<TaxonomyResponse>> CreateCategory(TaxonomyRequest request, CancellationToken cancellationToken)
    {
        var slug = SlugService.Create(request.Name);
        if (string.IsNullOrEmpty(slug) || await dbContext.Categories.AnyAsync(item => item.Slug == slug, cancellationToken))
        {
            return Conflict(new { message = "A category with this name already exists." });
        }
        var category = new Category { Name = request.Name.Trim(), Slug = slug };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetCategories), null, new TaxonomyResponse(category.Id, category.Name, category.Slug));
    }
}
