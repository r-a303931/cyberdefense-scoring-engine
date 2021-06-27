using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.CompetitionTasks
{
    public class DetailsModel : PageModel
    {
        private readonly EngineController.Data.EngineControllerContext _context;

        public DetailsModel(EngineController.Data.EngineControllerContext context)
        {
            _context = context;
        }

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
    }
}
