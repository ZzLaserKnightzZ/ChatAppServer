using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("/[controller]/[action]")]
    public class MauiTestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Test()
        {
            return Ok(new {hi="hi"});
        }
    }
}
