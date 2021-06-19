using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.CompetitionPenalties
{
    public class CreateModel : PageModel
    {
        private readonly EngineControllerContext _context;

        public CreateModel(EngineControllerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(int? SystemIdentifier)
        {
            if (SystemIdentifier == null)
            {
                return NotFound();
            }

            if (!_context.CompetitionSystems.Any(s => s.ID == SystemIdentifier))
            {
                return NotFound();
            }

            NewPenaltySystemIdentifier = SystemIdentifier;

            return Page();
        }

        [BindProperty]
        public CompetitionPenalty CompetitionPenalty { get; set; }

        [BindProperty]
        public int? NewPenaltySystemIdentifier { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            CompetitionPenalty.SystemIdentifier = (int)NewPenaltySystemIdentifier;

            _context.CompetitionPenalty.Add(CompetitionPenalty);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
