using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.SupplierOrders
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public List<SupplierOrder> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        public async Task OnGetAsync()
        {
            var query = _db.SupplierOrders
                .Include(o => o.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Q))
            {
                string search = Q.ToLower().Trim();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(search) ||
                    o.Supplier.Name.ToLower().Contains(search)
                );
            }

            if (!string.IsNullOrWhiteSpace(Status))
            {
                query = query.Where(o => o.Status == Status);
            }

            Orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }
    }
}