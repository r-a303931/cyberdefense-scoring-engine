using Microsoft.AspNetCore.Mvc.RazorPages;

using ClientCommon.Data.InformationContext;

namespace ClientCommon.WebInterface.Pages
{
    public class READMEModel : PageModel
    {
        private readonly IClientInformationContext _informationContext;

        public READMEModel(IClientInformationContext informationContext)
        {
            _informationContext = informationContext;
        }

        public string ReadmeHTML { get; set; }

        public async void OnGet()
        {
            var systemInfo = await _informationContext.GetSystem();

            var MarkdownConverter = new MarkdownSharp.Markdown();
            ReadmeHTML = MarkdownConverter.Transform(systemInfo.ReadmeText);
        }
    }
}
