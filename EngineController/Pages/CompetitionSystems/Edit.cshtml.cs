using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.CompetitionSystems
{
    public class EditModel : PageModel
    {
        private readonly EngineController.Data.EngineControllerContext _context;

        public EditModel(EngineController.Data.EngineControllerContext context)
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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(CompetitionSystem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompetitionSystemExists(CompetitionSystem.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool CompetitionSystemExists(int id)
        {
            return _context.CompetitionSystems.Any(e => e.ID == id);
        }
    }
}
