using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using EngineController.Models;

namespace EngineController.Pages.CompetitionSystems
{
    public class IndexModel : PageModel
    {
        private readonly EngineController.Data.EngineControllerContext _context;

        public IndexModel(EngineController.Data.EngineControllerContext context)
        {
            _context = context;
        }

        public IList<CompetitionSystem> CompetitionSystem { get; set; }

        public async Task OnGetAsync()
        {
            CompetitionSystem = await _context.CompetitionSystems.ToListAsync();
        }
    }
}
