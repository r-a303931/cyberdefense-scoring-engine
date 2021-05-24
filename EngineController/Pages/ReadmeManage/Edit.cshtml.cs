using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.ReadmeManage
{
    public class ReadmeEditModel : PageModel
    {
        private readonly EngineControllerContext _context;

        public ReadmeEditModel(EngineControllerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Readme Readme { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Readme = await _context.Readme.FindAsync(1);

            if (Readme == null)
            {
                Readme = new Readme
                {
                    ID = 1,
                    Text = ""
                };
            }

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (await ReadmeExists())
			{
				if (!ModelState.IsValid)
				{
					return Page();
				}

                _context.Attach(Readme).State = EntityState.Modified;
				await _context.SaveChangesAsync();

				return RedirectToPage("./Index");
			}
            else
			{
				if (!ModelState.IsValid)
				{
					return Page();
				}

				_context.Readme.Add(Readme);
				await _context.SaveChangesAsync();

				return RedirectToPage("./Index");
			}
        }

        private async Task<bool> ReadmeExists()
        {
            return (await _context.Readme.AsNoTracking().FirstOrDefaultAsync(v => v.ID == 1)) != null;
        }
    }
}
