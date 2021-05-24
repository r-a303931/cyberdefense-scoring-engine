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

namespace EngineController.Controllers
{
	[ApiController]
	[Route("api/readme")]
	[Produces("text/plain")]
	public class ReadmeController : Controller
	{
		private readonly EngineControllerContext _context;

		public ReadmeController(EngineControllerContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<string> Index()
		{
			var readme = await _context.Readme.FindAsync(1) ?? new Readme
			{
				ID = 1,
				Text = ""
			};

			return readme.Text;
		}
	}
}
