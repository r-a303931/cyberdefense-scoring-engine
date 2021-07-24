using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Common.Models;

using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;

namespace ClientCommon.WebInterface.Pages
{
    public class RegisterTeamModel : PageModel
    {
        private readonly IClientInformationContext _informationContext;
        private readonly IConfigurationManager _configurationManager;

        public RegisterTeamModel(IClientInformationContext informationContext, IConfigurationManager configurationManager)
        {
            _informationContext = informationContext;
            _configurationManager = configurationManager;
        }

        public IEnumerable<Team> Teams { get; set; }

        [BindProperty]
        public int TeamId { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            var config = await _configurationManager.LoadConfiguration(cancellationToken);

            if (config.SystemGUID != default)
            {
                return RedirectToPage("./README");
            }

            Teams = await _informationContext.GetTeamsAsync(cancellationToken);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            var teams = await _informationContext.GetTeamsAsync(cancellationToken);

            if (teams.First(t => t.ID == TeamId) == null)
            {
                return Page();
            }

            var config = await _configurationManager.LoadConfiguration(cancellationToken);
            config.SystemGUID = Guid.NewGuid();
            config.TeamID = TeamId;
            await _configurationManager.SaveConfiguration(config, cancellationToken);

            await _informationContext.RegisterVM(cancellationToken);

            return RedirectToPage("./README");
        }
    }
}
