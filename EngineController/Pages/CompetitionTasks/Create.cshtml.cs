using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.CompetitionTasks
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

            NewTaskSystemIdentifier = SystemIdentifier;

            return Page();
        }

        [BindProperty]
        public CompetitionTask CompetitionTask { get; set; }

        [BindProperty]
        public int? NewTaskSystemIdentifier { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || NewTaskSystemIdentifier == null)
            {
                return Page();
            }

            CompetitionTask.SystemIdentifier = (int)NewTaskSystemIdentifier;

            _context.CompetitionTask.Add(CompetitionTask);
            await _context.SaveChangesAsync();

            return RedirectToPage("../CompetitionSystems/Details", new { id = NewTaskSystemIdentifier });
        }
    }
}
