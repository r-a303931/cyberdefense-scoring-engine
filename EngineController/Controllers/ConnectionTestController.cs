using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EngineController.Controllers
{
	[ApiController]
	[Route("api/connection")]
	[Produces("application/json")]
    public class ConnectionTestController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return new JsonResult(new { Success = true });
        }
    }
}
