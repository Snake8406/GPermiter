using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GPermitter.Pages.EndUser;

[Authorize(Roles = "EndUser,Admin")]
public class DashboardModel : PageModel
{
    public void OnGet()
    {
    }
}
