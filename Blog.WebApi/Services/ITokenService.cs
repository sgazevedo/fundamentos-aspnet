using Blog.Models;

namespace Blog.WebApi.Services
{
  public interface ITokenService
  {
    string GenerateToken(User user);
  }
}
