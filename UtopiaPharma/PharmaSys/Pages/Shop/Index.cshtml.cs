using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Services;

namespace PharmaSys.Pages.Shop
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        private readonly CartService _cart;

        public IndexModel(PharmaSysDbContext db, CartService cart)
        {
            _db = db;
            _cart = cart;
        }

        [BindProperty(SupportsGet = true)] public string? Q { get; set; }
        [BindProperty(SupportsGet = true)] public int? CatId { get; set; }

        public int CartCount => _cart.GetCart().Sum(x => x.Quantity);

        public List<CategoryVm> Categories { get; set; } = new();
        public List<ProductVm> Products { get; set; } = new();

        public class CategoryVm
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        public class ProductVm
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string CategoryName { get; set; } = "";
            public decimal Price { get; set; }
        }

        public async Task OnGetAsync()
        {
            Categories = await _db.Categories
                .AsNoTracking()
                .Select(c => new CategoryVm { Id = c.Id, Name = c.Name! })
                .ToListAsync();

            var query = _db.Medications
                .AsNoTracking()
                .Include(m => m.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Q))
                query = query.Where(m => m.Name!.Contains(Q));

            if (CatId.HasValue)
                query = query.Where(m => m.CategoryId == CatId.Value);

            // ✅ Detecte le nom du champ prix existant (Price ou UnitPrice)
            string? priceProp = null;
            var t = typeof(PharmaSys.Models.Medication);

            if (t.GetProperty("Price") != null) priceProp = "Price";
            else if (t.GetProperty("UnitPrice") != null) priceProp = "UnitPrice";
            else if (t.GetProperty("SalePrice") != null) priceProp = "SalePrice";

            // ✅ IMPORTANT: pas de méthode GetPrice() dans Select EF
            if (priceProp != null)
            {
                Products = await query
                    .OrderBy(m => m.Name)
                    .Select(m => new ProductVm
                    {
                        Id = m.Id,
                        Name = m.Name!,
                        CategoryName = m.Category != null ? m.Category.Name! : "",
                        Price = EF.Property<decimal>(m, priceProp)
                    })
                    .ToListAsync();
            }
            else
            {
                // Aucun champ prix trouvé => prix=0 (à adapter)
                Products = await query
                    .OrderBy(m => m.Name)
                    .Select(m => new ProductVm
                    {
                        Id = m.Id,
                        Name = m.Name!,
                        CategoryName = m.Category != null ? m.Category.Name! : "",
                        Price = 0m
                    })
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostAddAsync(int id)
        {
            await _cart.AddAsync(id, 1);
            return RedirectToPage();
        }
    }
}