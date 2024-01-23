using System;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Controllers;

[ApiController]
public class GcCollectController : ControllerBase
{
    [ApiVersion("1.0")]
    [HttpGet("/api/v{version:apiVersion}/gc-collect")]
    public IActionResult Get()
    {
        GC.Collect();
        return Ok("Garbage Collected");
    }
}
