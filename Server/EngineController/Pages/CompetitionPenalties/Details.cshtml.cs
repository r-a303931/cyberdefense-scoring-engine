using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Models;

namespace EngineController.Pages.CompetitionPenalties
{
    public class DetailsModel : PageModel
    {
        private readonly Data.EngineControllerContext _context;

        public DetailsModel(Data.EngineControllerContext context)
        {
            _context = context;
        }

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
    }
}
