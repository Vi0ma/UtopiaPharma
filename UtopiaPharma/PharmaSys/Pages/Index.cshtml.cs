using Microsoft.AspNetCore.Authorization; //  Indispensable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaSys.Pages
{
    [AllowAnonymous] //  C'EST ICI LE SECRET : Tout le monde peut voir cette page
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Si l'admin est déjà connecté, on peut le rediriger directement vers le dashboard (optionnel)
            // if (User.Identity.IsAuthenticated && User.IsInRole("Admin")) 
            // {
            //     Response.Redirect("/Dashboard/Index");
            // }
        }
    }
}