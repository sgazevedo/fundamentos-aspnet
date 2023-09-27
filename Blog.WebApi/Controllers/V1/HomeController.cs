using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApi.Controllers.V1
{
  [ApiController]
  [Route("v1")]
  public class HomeController : ControllerBase
  {
    [HttpGet("health-check")]
    public IActionResult Get() => Ok();
  }
}
