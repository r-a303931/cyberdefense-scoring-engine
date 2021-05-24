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
    public class IndexModel : PageModel
    {
        private readonly EngineController.Data.EngineControllerContext _context;

        public IndexModel(EngineController.Data.EngineControllerContext context)
        {
            _context = context;
        }

        public IList<CompetitionTask> CompetitionTask { get;set; }

        public async Task OnGetAsync()
        {
            CompetitionTask = await _context.CompetitionTask.ToListAsync();
        }
    }
}
