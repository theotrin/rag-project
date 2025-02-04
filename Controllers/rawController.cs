using Microsoft.AspNetCore.Mvc;
using StartApi;

namespace StartApi.Controllers;

[ApiController]
 [Route("api/[controller]")]
public class RawController : ControllerBase
{
    [HttpGet("GetRaw")]
    public IActionResult GetRaw()
    {
       return Ok("Raw data");
    }
}
