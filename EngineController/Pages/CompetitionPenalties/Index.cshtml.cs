using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.CompetitionPenalties
{
    public class IndexModel : PageModel
    {
        private readonly EngineControllerContext _context;

        public IndexModel(EngineControllerContext context)
        {
            _context = context;
        }

        public IList<CompetitionPenalty> CompetitionPenalty { get;set; }

        public async Task OnGetAsync()
        {
            CompetitionPenalty = await _context.CompetitionPenalty.ToListAsync();
        }
    }
}
