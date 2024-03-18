using MoYuRazorApi.Application;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoYuRazorApi.Web.Entry.Pages
{
    public class IndexModel : PageModel
    {
        public readonly ISystemService _systemService;

        public IndexModel(ISystemService systemService)
        {
            _systemService = systemService;
        }

        public void OnGet()
        {
            ViewData["Description"] = _systemService.GetDescription();
        }
    }
}
