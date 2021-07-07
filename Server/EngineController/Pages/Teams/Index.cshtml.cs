using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.Teams
{
    public class IndexModel : PageModel
    {
        private readonly EngineControllerContext _context;

        public IndexModel(EngineControllerContext context)
        {
            _context = context;
        }

        public IList<Team> Team { get; set; }

        public async Task OnGetAsync()
        {
            Team = await _context.Teams
                .Include(t => t.AppliedCompetitionPenalties)
                .ThenInclude(p => p.CompetitionPenalty)
                .Include(t => t.CompletedCompetitionTasks)
                .ThenInclude(p => p.CompetitionTask)
                .ToListAsync();
        }
    }
}
