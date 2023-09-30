using System.ComponentModel.DataAnnotations;

namespace Blog.WebApi.ViewModels.Accounts
{
  public class UploadImageViewModel
  {
    [Required(ErrorMessage = "Imagem inválida")]
    public string Base64Image { get; set; }
  }
}
