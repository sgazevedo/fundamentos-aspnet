using System.ComponentModel.DataAnnotations;

namespace Blog.WebApi.ViewModels.Accounts
{
  public class RegisterViewModel
  {
    [Required(ErrorMessage = "O campo 'Name' é obrigatório.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "O campo 'Email' é obrigatório.")]
    [EmailAddress(ErrorMessage = "O Email informado é inválido.")]
    public string Email { get; set; }
  }
}
