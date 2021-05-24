using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;
using EngineController.Models;

namespace EngineController.Pages.ReadmeManage
{
    public class IndexModel : PageModel
    {
        private readonly EngineControllerContext _context;

        public IndexModel(EngineControllerContext context)
        {
            _context = context;
        }

        public Readme Readme { get; set; }

        public string ReadmeHTML { get; set; }

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

            var MarkdownConverter = new MarkdownSharp.Markdown();
            ReadmeHTML = MarkdownConverter.Transform(Readme.Text);

            return Page();
        }
    }
}
