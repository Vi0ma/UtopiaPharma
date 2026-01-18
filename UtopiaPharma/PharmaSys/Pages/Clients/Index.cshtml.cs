using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public List<Client> Clients { get; set; } = new();
        public Dictionary<int, decimal> TotalPurchases { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; } // Pour la recherche

        public async Task OnGetAsync()
        {
            var query = _db.Clients.AsQueryable();

            //  RECHERCHE
            if (!string.IsNullOrWhiteSpace(Q))
            {
                string search = Q.ToLower().Trim();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(search) ||
                    c.Code.ToLower().Contains(search) ||
                    (c.Phone != null && c.Phone.Contains(search))
                );
            }

            Clients = await query
                .OrderByDescending(c => c.LastVisit ?? DateTime.MinValue)
                .ToListAsync();

            // Total achats = somme des Sales.Total par client
            // (Note: on charge tout pour l'instant, c'est acceptable si < 1000 clients)
            var clientIds = Clients.Select(c => c.Id).ToList();

            TotalPurchases = await _db.Sales
                .Where(s => s.ClientId != null && clientIds.Contains(s.ClientId.Value))
                .GroupBy(s => s.ClientId!.Value)
                .Select(g => new { ClientId = g.Key, Total = g.Sum(x => x.Total) })
                .ToDictionaryAsync(x => x.ClientId, x => x.Total);
        }
    }
}