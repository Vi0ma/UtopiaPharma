using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Clients
{
    public class DeleteModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public DeleteModel(PharmaSysDbContext db) => _db = db;

        [BindProperty]
        public Client Client { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var c = await _db.Clients.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return RedirectToPage("Index");
            Client = c;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var c = await _db.Clients.FindAsync(id);
            if (c == null) return RedirectToPage("Index");

            //  SÉCURITÉ : Vérifier si le client a un historique d'achats (Ventes magasin)
            // C'est la vérification la plus importante.
            bool hasSales = await _db.Sales.AnyAsync(s => s.ClientId == id);

            if (hasSales)
            {
                // On empêche la suppression
                ModelState.AddModelError("", "Impossible de supprimer ce client car il possède un historique d'achats.");
                Client = c; // On recharge les données pour l'affichage
                return Page();
            }

            _db.Clients.Remove(c);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}