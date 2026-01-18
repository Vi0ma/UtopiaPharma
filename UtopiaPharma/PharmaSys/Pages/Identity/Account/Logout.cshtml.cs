using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaSys.Data; // Vérifiez que c'est bien là où est ApplicationUser

namespace PharmaSys.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        //  C'EST ICI LE SECRET : On force l'utilisation de ApplicationUser
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // 1. Déconnexion réelle
            await _signInManager.SignOutAsync();

            _logger.LogInformation("Utilisateur déconnecté avec succès.");

            // 2. Redirection
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Si pas d'URL précise, on renvoie vers l'accueil ou la page de connexion
                return RedirectToPage("/Index");
            }
        }
    }
}