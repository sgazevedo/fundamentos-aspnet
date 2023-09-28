using System.ComponentModel.DataAnnotations;

namespace Blog.WebApi.ViewModels.Categories
{
  public class EditorCategoryViewModel
  {
    [Required(ErrorMessage = "O campo 'Name' é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O campo 'Name' deve conter no mínimo 3 e no máximo 40 caracteres.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "O campo 'Slug' é obrigatório.")]
    public string Slug { get; set; }
  }
}
