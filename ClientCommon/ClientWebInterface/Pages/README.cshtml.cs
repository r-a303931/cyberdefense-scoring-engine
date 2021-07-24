using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;

namespace ClientCommon.WebInterface.Pages
{
    public class READMEModel : PageModel
    {
        private readonly IClientInformationContext _informationContext;
        private readonly IConfigurationManager _configurationManager;

        public READMEModel(IClientInformationContext informationContext, IConfigurationManager configurationManager)
        {
            _informationContext = informationContext;
            _configurationManager = configurationManager;
        }

        public string ReadmeHTML { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            var config = await _configurationManager.LoadConfiguration(cancellationToken);

            if (config.SystemGUID == default)
            {
                return RedirectToPage("./RegisterTeam");
            }

            var system = await _informationContext.GetSystem(cancellationToken);

            var converter = new MarkdownSharp.Markdown();
            ReadmeHTML = converter.Transform(system.ReadmeText);

            return Page();
        }
    }
}
