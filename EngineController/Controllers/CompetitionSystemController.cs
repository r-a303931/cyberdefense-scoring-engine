using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EngineController.Data;
using System.Net;
using System.Net.Http;
using EngineController.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineController.Controllers
{
	[ApiController]
	[Route("api/systems")]
	[Produces("application/json")]
	public class CompetitionSystemController : Controller
	{
		private readonly EngineControllerContext _context;

		public CompetitionSystemController(EngineControllerContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			return new JsonResult(
				await _context.CompetitionSystems
					.Include(s => s.CompetitionPenalties)
					.Include(s => s.CompetitionTasks)
					.ToListAsync()
			);
		}
	}
}
