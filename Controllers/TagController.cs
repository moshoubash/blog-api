using DotnetAPI.Database;
using DotnetAPI.Dtos.Blog;
using DotnetAPI.Models;
using DotnetAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("api/tags")]
public sealed class TagController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaxonomyResponse>>> GetTags(CancellationToken cancellationToken) =>
        Ok(await dbContext.Tags.AsNoTracking().OrderBy(item => item.Name)
            .Select(item => new TaxonomyResponse(item.Id, item.Name, item.Slug)).ToListAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Author")]
    public async Task<ActionResult<TaxonomyResponse>> CreateTag(TaxonomyRequest request, CancellationToken cancellationToken)
    {
        var slug = SlugService.Create(request.Name);
        
        if (string.IsNullOrEmpty(slug) || await dbContext.Tags.AnyAsync(item => item.Slug == slug, cancellationToken))
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "A tag with this name already exists.");
        }
        
        var tag = new Tag { 
            Name = request.Name.Trim(), 
            Slug = slug 
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return CreatedAtAction(nameof(GetTags), null, new TaxonomyResponse(tag.Id, tag.Name, tag.Slug));
    }
}
