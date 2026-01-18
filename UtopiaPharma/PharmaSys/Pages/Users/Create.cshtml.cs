using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaSys.Data; // Vérifiez que ApplicationUser est accessible
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        //  CORRECTION : ApplicationUser ici aussi
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = "";

            [Required, StringLength(100, MinimumLength = 6)]
            public string Password { get; set; } = "";

            [Required]
            public string Role { get; set; } = "Pharmacien";
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            //  Création d'un ApplicationUser
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true
                // Vous pouvez ajouter ici : FullName = "Nom par défaut" si votre classe l'exige
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(Input.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Input.Role));
                }

                await _userManager.AddToRoleAsync(user, Input.Role);
                return RedirectToPage("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}