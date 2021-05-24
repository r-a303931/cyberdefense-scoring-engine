using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.Teams
{
    public class ResetModel : PageModel
    {
        private readonly EngineControllerContext _context;

        public ResetModel(EngineControllerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Team Team { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Team = await _context.Teams
                .Include(t => t.CompletedCompetitionTasks)
                .ThenInclude(t => t.CompetitionTask)
                .Include(t => t.AppliedCompetitionPenalties)
                .ThenInclude(p => p.CompetitionPenalty)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (Team == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Team = await _context.Teams
                .Include(t => t.AppliedCompetitionPenalties)
                .Include(t => t.CompletedCompetitionTasks)
                .FirstOrDefaultAsync(t => t.ID == id);

            if (Team != null)
            {
                foreach (var penalty in Team.AppliedCompetitionPenalties)
				{
                    Team.AppliedCompetitionPenalties.Remove(penalty);
				}
                foreach (var task in Team.CompletedCompetitionTasks)
				{
                    Team.CompletedCompetitionTasks.Remove(task);
				}

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
