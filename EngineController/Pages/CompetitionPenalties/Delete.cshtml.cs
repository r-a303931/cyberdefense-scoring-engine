using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.CompetitionPenalties
{
    public class DeleteModel : PageModel
    {
        private readonly EngineController.Data.EngineControllerContext _context;

        public DeleteModel(EngineController.Data.EngineControllerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CompetitionPenalty CompetitionPenalty { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CompetitionPenalty = await _context.CompetitionPenalty.FirstOrDefaultAsync(m => m.ID == id);

            if (CompetitionPenalty == null)
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

            CompetitionPenalty = await _context.CompetitionPenalty.FindAsync(id);

            if (CompetitionPenalty != null)
            {
                _context.CompetitionPenalty.Remove(CompetitionPenalty);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
