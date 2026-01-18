using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Alerts
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        // Liste des médicaments en rupture
        public List<Medication> LowStockMeds { get; set; } = new();

        // Liste des médicaments qui expirent bientôt
        public List<Medication> ExpiringMeds { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 1. Récupérer TOUS les médicaments
            var allMeds = await _db.Medications
                .Include(m => m.Supplier)
                .AsNoTracking()
                .ToListAsync();

            // 2. Filtrer : Stock Faible (Stock <= StockMin)
            LowStockMeds = allMeds
                .Where(m => m.Stock <= m.StockMin)
                .OrderBy(m => m.Stock) // Les plus urgents (0 stock) en premier
                .ToList();

            // 3. Filtrer : Expiration (Périmés OU périment dans les 90 jours)
            var threshold = DateTime.Today.AddDays(90); // Alerte 3 mois avant
            
            ExpiringMeds = allMeds
                .Where(m => m.ExpirationDate.HasValue && m.ExpirationDate.Value <= threshold)
                .OrderBy(m => m.ExpirationDate) // Les périmés en premier
                .ToList();
        }
    }
}