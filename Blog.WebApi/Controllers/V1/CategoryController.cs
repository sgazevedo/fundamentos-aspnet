using Blog.Data;
using Blog.Models;
using Blog.WebApi.Extensions;
using Blog.WebApi.ViewModels;
using Blog.WebApi.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.WebApi.Controllers.V1
{
  [ApiController]
  [Route("v1")]
  public class CategoryController : ControllerBase
  {
    private readonly BlogDataContext _blogDataContext;
    private readonly IMemoryCache _cache;

    public CategoryController(BlogDataContext blogDataContext, IMemoryCache cache)
    {
      _blogDataContext = blogDataContext;
      _cache = cache;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetAsync()
    {
      try
      {
        var categories = await _cache.GetOrCreate("CategoriesCache", entry =>
        {
          entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
          return GetCategories();
        });

        return Ok(new ResultViewModel<IEnumerable<Category>>(categories));
      }
      catch 
      {
        return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna no servidor."));
      }
    }

    private async Task<IEnumerable<Category>> GetCategories() 
    {
      var categories = await _blogDataContext.Categories.ToListAsync();
      return categories;
    }

    [HttpGet("categories/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
      try
      {
        var category = await _cache.GetOrCreate($"{id}-{nameof(Category)}", entry =>
        {
          entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
          return _blogDataContext.Categories
          .AsNoTracking()
          .FirstOrDefaultAsync(x => x.Id == id);
        });

        if (category is null)
          return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado."));

        return Ok(new ResultViewModel<Category>(category));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor."));
      }

    }

    [HttpPost("categories")]
    public async Task<IActionResult> PostAsync([FromBody] EditorCategoryViewModel model)
    {
      if (!ModelState.IsValid)
        return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

      try
      {
        var category = new Category
        {
          Name = model.Name,
          Slug = model.Slug.ToLower()
        };

        await _blogDataContext.Categories.AddAsync(category);
        await _blogDataContext.SaveChangesAsync();

        return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
      }
      catch (DbUpdateException)
      {
        return StatusCode(500, new ResultViewModel<Category>("Não foi possível incluir a categoria."));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor."));

      }
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> PutAsync([FromRoute] int id, [FromBody] EditorCategoryViewModel model)
    {
      try
      {
        var category = await _blogDataContext.Categories
          .AsNoTracking()
          .FirstOrDefaultAsync(x => x.Id == id);

        if (category is null)
          return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado."));

        category.Name = model.Name;
        category.Slug = model.Slug;

        _blogDataContext.Categories.Update(category);
        await _blogDataContext.SaveChangesAsync();

        return Ok(new ResultViewModel<Category>(category));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor."));
      }
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
      try
      {
        var category = await _blogDataContext.Categories
          .AsNoTracking()
          .FirstOrDefaultAsync(x => x.Id == id);

        if (category is null)
          return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado."));

        _blogDataContext.Categories.Remove(category);
        await _blogDataContext.SaveChangesAsync();

        return Ok(new ResultViewModel<Category>(category));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor."));
      }
    }
  }
}
