using System;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Controllers
{
    [ApiController]
    public class GcCollectController : ControllerBase
    {
        [HttpGet("/api/v1/gc-collect")]
        public IActionResult Get()
        {
            GC.Collect();
            return Ok("Garbage Collected");
        }
    }
}
