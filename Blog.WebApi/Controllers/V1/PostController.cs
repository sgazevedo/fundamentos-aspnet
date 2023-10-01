using Blog.Data;
using Blog.Models;
using Blog.WebApi.Extensions;
using Blog.WebApi.ViewModels;
using Blog.WebApi.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.WebApi.Controllers.V1
{
  [ApiController]
  public class PostController : ControllerBase
  {
    private readonly BlogDataContext _context;

    public PostController(BlogDataContext context)
    {
      _context = context;
    }

    [HttpGet("v1/posts")]
    public async Task<IActionResult> GetAsync(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 25)
    {
      try
      {
        var count = await _context.Posts.AsNoTracking().CountAsync();
        var posts = await _context
            .Posts
            .AsNoTracking()
            .Include(x => x.Author)
            .Include(x => x.Category)
            .Select(x => new ListPostsViewModel
            {
              Id = x.Id,
              Title = x.Title,
              Slug = x.Slug,
              LastUpdateDate = x.LastUpdateDate,
              Category = x.Category.Name,
              Author = $"{x.Author.Name} ({x.Author.Email})"
            })
            .Skip(page * pageSize)
            .Take(pageSize)
            .OrderByDescending(x => x.LastUpdateDate)
            .ToListAsync();
        return Ok(new ResultViewModel<dynamic>(new
        {
          total = count,
          page,
          pageSize,
          posts
        }));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<List<Category>>("05X04 - Falha interna no servidor"));
      }
    }

    [HttpGet("v1/posts/{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
      try
      {
        var post = await _context
            .Posts
            .AsNoTracking()
            .Include(x => x.Author)
            .Include(x => x.Category)
            .Select(x => new ListPostsViewModel
            {
              Id = x.Id,
              Title = x.Title,
              Slug = x.Slug,
              LastUpdateDate = x.LastUpdateDate,
              Category = x.Category.Name,
              Author = $"{x.Author.Name} ({x.Author.Email})"
            })
            .FirstOrDefaultAsync(x => x.Id == id);

        if (post is null)
          return NotFound(new ResultViewModel<Post>("Conteúdo não encontrado"));
        
        return Ok(new ResultViewModel<ListPostsViewModel>(post));
      }
      catch (Exception)
      {
        return StatusCode(500, new ResultViewModel<List<Category>>("05X04 - Falha interna no servidor"));
      }
    }

    [HttpGet("v1/posts/category/{category}")]
    public async Task<IActionResult> GetByCategoryAsync(
        [FromRoute] string category,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 25)
    {
      try
      {
        var count = await _context.Posts.AsNoTracking().CountAsync();
        var posts = await _context
            .Posts
            .AsNoTracking()
            .Include(x => x.Author)
            .Include(x => x.Category)
            .Where(x => x.Category.Slug == category)
            .Select(x => new ListPostsViewModel
            {
              Id = x.Id,
              Title = x.Title,
              Slug = x.Slug,
              LastUpdateDate = x.LastUpdateDate,
              Category = x.Category.Name,
              Author = $"{x.Author.Name} ({x.Author.Email})"
            })
            .Skip(page * pageSize)
            .Take(pageSize)
            .OrderByDescending(x => x.LastUpdateDate)
            .ToListAsync();
        return Ok(new ResultViewModel<dynamic>(new
        {
          total = count,
          page,
          pageSize,
          posts
        }));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<List<Category>>("05X04 - Falha interna no servidor"));
      }
    }

    [Authorize(Roles = "author")]
    [HttpPost("v1/posts")]
    public async Task<IActionResult> PostAync([FromBody] EditorPostViewModel model)
    {
      if (!ModelState.IsValid)
        return BadRequest(new ResultViewModel<dynamic>(ModelState.GetErrors()));

      try
      {
        var userIdentity = User.Identity?.Name ?? string.Empty;

        var user = await _context
            .Users
            .FirstOrDefaultAsync(x => x.Email.Equals(userIdentity));

        if (user is null)
          return NotFound(new ResultViewModel<Category>("Usuário não encontrado"));

        var category = await _context
          .Categories
          .AsNoTracking()
          .FirstOrDefaultAsync(x => x.Name.Equals(model.Category));

        if (category is null)
          return NotFound(new ResultViewModel<Category>("Categoria não encontrada"));

        var post = new Post
        {
          CategoryId = category.Id,
          AuthorId = user.Id,
          Title = model.Title,
          Summary = model.Summary,
          Body = model.Body,
          Slug = model.Title.Replace(" ", "-").ToLowerInvariant(),
          CreateDate = DateTime.UtcNow,
          LastUpdateDate = DateTime.UtcNow
        };


        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var postViewModel = new ListPostsViewModel
        {
          Id = post.Id,
          Title = post.Title,
          Slug = post.Slug,
          LastUpdateDate = post.LastUpdateDate,
          Category = category.Name,
          Author = $"{user.Name} ({user.Email})"
        };

        return Ok(new ResultViewModel<ListPostsViewModel>(postViewModel));
      }
      catch (DbUpdateException)
      {
        return StatusCode(400, new ResultViewModel<string>("05X99 - Erro inesperado"));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
      }
    }

    [Authorize(Roles = "author")]
    [HttpPut("v1/posts/{id:int}")]
    public async Task<IActionResult> PutAync([FromRoute] int id, [FromBody] EditorPostViewModel model)
    {
      if (!ModelState.IsValid)
        return BadRequest(new ResultViewModel<dynamic>(ModelState.GetErrors()));

      try
      {
        var userIdentity = User.Identity?.Name ?? string.Empty;

        var user = await _context.Users
          .AsNoTracking()
          .FirstOrDefaultAsync(x => x.Email.Equals(userIdentity));

        if (user is null)
          return NotFound(new ResultViewModel<Category>("Usuário não encontrado"));

        var post = await _context.Posts
          .AsNoTracking()
          .FirstOrDefaultAsync(x => x.Id == id);

        if (post is null)
          return NotFound(new ResultViewModel<Post>("Conteúdo não encontrado."));

        var category = await _context.Categories
          .AsNoTracking()
          .FirstOrDefaultAsync(x => x.Name.Equals(model.Category));

        if (category is null)
          return NotFound(new ResultViewModel<Category>("Categoria não encontrada"));

        post.CategoryId = category.Id;
        post.Title = model.Title;
        post.Summary = model.Summary;
        post.Body = model.Body;
        post.Slug = model.Title.Replace(" ", "-").ToLowerInvariant();
        post.LastUpdateDate = DateTime.UtcNow;


        _context.Posts.Update(post);
        await _context.SaveChangesAsync();

        var postViewModel = new ListPostsViewModel
        {
          Id = post.Id,
          Title = post.Title,
          Slug = post.Slug,
          LastUpdateDate = post.LastUpdateDate,
          Category = category.Name,
          Author = $"{user.Name} ({user.Email})"
        };

        return Ok(new ResultViewModel<ListPostsViewModel>(postViewModel));
      }
      catch (DbUpdateException)
      {
        return StatusCode(400, new ResultViewModel<string>("05X99 - Erro inesperado"));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
      }
    }

    [Authorize(Roles = "author")]
    [HttpDelete("v1/posts/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
      try
      {
        var userIdentity = User.Identity?.Name ?? string.Empty;

        var user = await _context.Users
          .AsNoTracking()
          .FirstOrDefaultAsync(x => x.Email.Equals(userIdentity));

        if (user is null)
          return NotFound(new ResultViewModel<Category>("Usuário não encontrado"));

        var post = await _context.Posts
          .AsNoTracking() 
          .FirstOrDefaultAsync(x => x.Id == id);

        if (post is null)
          return NotFound(new ResultViewModel<Post>("Conteúdo não encontrado."));

        if (post.AuthorId != user.Id)
          return BadRequest(new ResultViewModel<Post>("Não é possível remover esse post."));

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
      }
      catch (DbUpdateException)
      {
        return StatusCode(400, new ResultViewModel<string>("05X99 - Erro inesperado"));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
      }
    }
  }
}
