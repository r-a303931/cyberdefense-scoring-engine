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
    public class DeleteModel : PageModel
    {
        private readonly EngineController.Data.EngineControllerContext _context;

        public DeleteModel(EngineController.Data.EngineControllerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CompetitionSystem CompetitionSystem { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CompetitionSystem = await _context.CompetitionSystems.FirstOrDefaultAsync(m => m.ID == id);

            if (CompetitionSystem == null)
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

            CompetitionSystem = await _context.CompetitionSystems.FindAsync(id);

            if (CompetitionSystem != null)
            {
                _context.CompetitionSystems.Remove(CompetitionSystem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
