using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Sales
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public List<Sale> Sales { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; } // Recherche texte

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFilter { get; set; } // Filtre Date

        public async Task OnGetAsync()
        {
            var query = _db.Sales
                .Include(s => s.Client)
                .AsQueryable();

            // 1. Recherche Texte (Facture ou Client)
            if (!string.IsNullOrWhiteSpace(Q))
            {
                string search = Q.ToLower().Trim();
                query = query.Where(s =>
                    s.InvoiceNo.ToLower().Contains(search) ||
                    (s.Client != null && s.Client.FullName.ToLower().Contains(search))
                );
            }

            // 2. Filtre Date (Optionnel)
            if (DateFilter.HasValue)
            {
                // On cherche les ventes de CE jour-là (ignorer l'heure)
                query = query.Where(s => s.SaleDate.Date == DateFilter.Value.Date);
            }

            // 3. Tri (Plus récent d'abord)
            Sales = await query
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }
    }
}