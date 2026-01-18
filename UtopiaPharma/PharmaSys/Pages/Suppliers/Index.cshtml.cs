using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Suppliers
{
    [Authorize(Roles = "Admin,Pharmacien")]
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public List<Supplier> Suppliers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        public async Task OnGetAsync()
        {
            var query = _db.Suppliers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Q))
            {
                string search = Q.ToLower().Trim();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(search) ||
                    (s.Phone != null && s.Phone.Contains(search)) ||
                    (s.Email != null && s.Email.ToLower().Contains(search))
                );
            }

            Suppliers = await query
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}