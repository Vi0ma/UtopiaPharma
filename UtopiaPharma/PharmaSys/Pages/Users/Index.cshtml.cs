using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data; // Assurez-vous que ApplicationUser est ici (ou dans PharmaSys.Models)

namespace PharmaSys.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        // CORRECTION ICI : On remplace <IdentityUser> par <ApplicationUser>
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public class UserViewModel
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }

        public List<UserViewModel> UsersWithRoles { get; set; } = new();

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            UsersWithRoles = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                UsersWithRoles.Add(new UserViewModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "Aucun"
                });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToPage();
        }
    }
}