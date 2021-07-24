using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Models;

namespace EngineController.Pages.CompetitionTasks
{
    public class DeleteModel : PageModel
    {
        private readonly EngineController.Data.EngineControllerContext _context;

        public DeleteModel(EngineController.Data.EngineControllerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CompetitionTask CompetitionTask { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CompetitionTask = await _context.CompetitionTask.FirstOrDefaultAsync(m => m.ID == id);

            if (CompetitionTask == null)
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

            CompetitionTask = await _context.CompetitionTask.FindAsync(id);

            if (CompetitionTask != null)
            {
                _context.CompetitionTask.Remove(CompetitionTask);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("../CompetitionSystems/Details", new { id = CompetitionTask.SystemIdentifier });
        }
    }
}
