using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.CompetitionSystems
{
    public class CreateModel : PageModel
    {
        private readonly EngineController.Data.EngineControllerContext _context;

        public CreateModel(EngineController.Data.EngineControllerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public CompetitionSystem CompetitionSystem { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.CompetitionSystems.Add(CompetitionSystem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
