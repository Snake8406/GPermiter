using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GPermitter.Pages.User;

[Authorize(Roles = "User,Admin")]
public class DashboardModel : PageModel
{
    public void OnGet()
    {
    }
}
