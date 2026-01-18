using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.OrdersAdmin
{
    // Accessible par Admin et Pharmacien
    [Authorize(Roles = "Admin,Pharmacien")]
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;

        public IndexModel(PharmaSysDbContext db)
        {
            _db = db;
        }

        public List<Order> Orders { get; set; } = new();

        public async Task OnGetAsync()
        {
            // On récupère les commandes avec les infos de l'utilisateur
            Orders = await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}