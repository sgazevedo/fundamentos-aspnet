using Blog.Data;
using Blog.Models;
using Blog.WebApi.Extensions;
using Blog.WebApi.Services;
using Blog.WebApi.ViewModels;
using Blog.WebApi.ViewModels.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace Blog.WebApi.Controllers.V1
{
  [ApiController]
  public class AccountController : ControllerBase
  {
    private readonly BlogDataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AccountController(BlogDataContext blogDataContext, ITokenService tokenService, IEmailService emailService)
    {
      _context = blogDataContext;
      _tokenService = tokenService;
      _emailService = emailService;
    }

    [HttpPost("v1/accounts/")]
    public async Task<IActionResult> Post([FromBody] RegisterViewModel model)
    {
      if (!ModelState.IsValid)
        return BadRequest(new ResultViewModel<dynamic>(ModelState.GetErrors()));

      var user = new User
      {
        Name = model.Name,
        Email = model.Email,
        Slug = model.Email.Replace("@", "-").Replace(".", "-")
      };

      var password = PasswordGenerator.Generate(25);
      user.PasswordHash = PasswordHasher.Hash(password);

      try
      {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _emailService.Send(
          user.Name, 
          user.Email, 
          Configuration.Email.Subject, 
          Configuration.Email.Body.Replace("{password}", password), 
          Configuration.Email.FromName, 
          Configuration.Email.FromEmail);

        return Ok(new ResultViewModel<dynamic>(new
        {
          user = user.Email,
          password
        }));
      }
      catch (DbUpdateException)
      {
        return StatusCode(400, new ResultViewModel<string>("05X99 - Este E-mail já está cadastrado"));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
      }
    }

    [HttpPost("v1/accounts/login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
      if (!ModelState.IsValid)
        return BadRequest(new ResultViewModel<dynamic>(ModelState.GetErrors()));

      var user = await _context
            .Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == model.Email);

      if (user == null)
        return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

      if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
        return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

      try
      {
        var token = _tokenService.GenerateToken(user);
        return Ok(new ResultViewModel<string>(token, null));
      }
      catch
      {
        return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
      }
    }
  }
}
