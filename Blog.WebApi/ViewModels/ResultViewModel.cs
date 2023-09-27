using System.Text.Json.Serialization;

namespace Blog.WebApi.ViewModels
{
  public class ResultViewModel<T>
  {
    public ResultViewModel(T data, List<string> errors)
    {
      Data = data;
      Errors = errors?.Any() ?? false ? errors : null;
    }

    public ResultViewModel(T data)
    {
      Data = data;
      Errors = null;
    }

    public ResultViewModel(List<string> errors)
    {
      Errors = errors;
    }

    public ResultViewModel(string error)
    {
      Errors.Add(error);
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T Data { get; private set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Errors { get; private set; } = new();
  }
}
