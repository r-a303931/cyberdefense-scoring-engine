using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.CompetitionSystems
{
    public class DetailsModel : PageModel
    {
        private readonly EngineControllerContext _context;

        public DetailsModel(EngineControllerContext context)
        {
            _context = context;
        }

        public CompetitionSystem CompetitionSystem { get; set; }

        public string ReadmeHTML { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var query = _context.CompetitionSystems
                .Include(s => s.CompetitionTasks)
                .Include(s => s.CompetitionPenalties);

            var queryString = query.ToQueryString();

            CompetitionSystem = await query.FirstOrDefaultAsync(m => m.ID == id);

            if (CompetitionSystem == null)
            {
                return NotFound();
            }

            var MarkdownConverter = new MarkdownSharp.Markdown();
            ReadmeHTML = MarkdownConverter.Transform(CompetitionSystem.ReadmeText);

            return Page();
        }
    }
}
