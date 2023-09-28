using System.ComponentModel.DataAnnotations;

namespace Blog.WebApi.ViewModels.Accounts
{
  public class LoginViewModel
  {
    [Required(ErrorMessage = "O campo 'Email' é obrigatório.")]
    [EmailAddress(ErrorMessage = "O Email informado é inválido.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "O campo 'Password' é obrigatório.")]
    public string Password { get; set; }
  }
}
