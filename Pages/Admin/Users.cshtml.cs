using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GPermitter.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel(UserManager<IdentityUser> userManager) : PageModel
{
    private static readonly string[] AssignableRoles = ["EndUser", "User", "Admin"];

    [BindProperty]
    public CreateUserInput CreateInput { get; set; } = new();

    public List<UserListItem> Users { get; private set; } = [];

    public IEnumerable<SelectListItem> RoleOptions =>
        AssignableRoles.Select(role => new SelectListItem(role, role));

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        var user = new IdentityUser
        {
            UserName = CreateInput.Email,
            Email = CreateInput.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, CreateInput.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadUsersAsync();
            return Page();
        }

        await userManager.AddToRoleAsync(user, CreateInput.Role);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        var currentUserId = userManager.GetUserId(User);
        if (userId == currentUserId)
        {
            ModelState.AddModelError(string.Empty, "You cannot delete your own account.");
            await LoadUsersAsync();
            return Page();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await userManager.DeleteAsync(user);
        }

        return RedirectToPage();
    }

    private async Task LoadUsersAsync()
    {
        var users = await userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var currentUserId = userManager.GetUserId(User);

        Users = [];
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            Users.Add(new UserListItem
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "Unknown",
                Roles = roles.ToList(),
                CanDelete = user.Id != currentUserId
            });
        }
    }

    public class CreateUserInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";
    }

    public class UserListItem
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
        public bool CanDelete { get; set; }
    }
}
