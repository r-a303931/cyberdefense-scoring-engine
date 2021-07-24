﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.Teams
{
    public class DetailsModel : PageModel
    {
        private readonly EngineControllerContext _context;

        public DetailsModel(EngineControllerContext context)
        {
            _context = context;
        }

        public Team Team { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Team = await _context.Teams
                .Include(t => t.AppliedCompetitionPenalties)
                    .ThenInclude(p => p.CompetitionPenalty)

                .Include(t => t.CompletedCompetitionTasks)
                    .ThenInclude(p => p.CompetitionTask)

                .Include(t => t.RegisteredVirtualMachines)
                    .ThenInclude(r => r.CompetitionSystem)
                .Include(t => t.RegisteredVirtualMachines)
                    .ThenInclude(r => r.CompetitionPenalties)
                .Include(t => t.RegisteredVirtualMachines)
                    .ThenInclude(r => r.CompetitionTasks)
                
                .FirstOrDefaultAsync();

            if (Team == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
