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

namespace EngineController.Pages.CompetitionPenalties
{
    public class EditModel : PageModel
    {
        private readonly EngineControllerContext _context;

        public EditModel(EngineControllerContext context)
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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(CompetitionPenalty).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompetitionPenaltyExists(CompetitionPenalty.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("../CompetitionSystems/Details", new { id = CompetitionPenalty.SystemIdentifier });
        }

        private bool CompetitionPenaltyExists(int id)
        {
            return _context.CompetitionPenalty.Any(e => e.ID == id);
        }
    }
}
