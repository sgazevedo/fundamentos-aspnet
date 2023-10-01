using System.ComponentModel.DataAnnotations;

namespace Blog.WebApi.ViewModels.Posts
{
  public class EditorPostViewModel
  {
    [Required(ErrorMessage = "O campo 'Category' é obrigatório")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "O campo 'Category' deve conter no mínimo 3 e no máximo 80 caracteres.")]
    public string Category { get; set; }

    [Required(ErrorMessage = "O campo 'Title' é obrigatório")]
    [StringLength(160, MinimumLength = 3, ErrorMessage = "O campo 'Title' deve conter no mínimo 3 e no máximo 160 caracteres.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "O campo 'Summary' é obrigatório")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "O campo 'Summary' deve conter no mínimo 3 e no máximo 255 caracteres.")]
    public string Summary { get; set; }

    [Required(ErrorMessage = "O campo 'Body' é obrigatório")]
    [StringLength(4000, MinimumLength = 3, ErrorMessage = "O campo 'Summary' deve conter no mínimo 3 e no máximo 4000 caracteres.")]
    public string Body { get; set; }
  }
}
