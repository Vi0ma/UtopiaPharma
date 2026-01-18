using Microsoft.AspNetCore.Http;
using PharmaSys.Data;
using PharmaSys.Helpers;
using PharmaSys.Models;
using Microsoft.EntityFrameworkCore;

namespace PharmaSys.Services
{
    public class CartItemDto
    {
        public int MedicationId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class CartService
    {
        private const string CartKey = "PHARMASYS_CART";
        private readonly IHttpContextAccessor _http;
        private readonly PharmaSysDbContext _db;

        public CartService(IHttpContextAccessor http, PharmaSysDbContext db)
        {
            _http = http;
            _db = db;
        }

        private ISession Session => _http.HttpContext!.Session;

        public List<CartItemDto> GetCart()
        {
            return Session.GetObject<List<CartItemDto>>(CartKey) ?? new List<CartItemDto>();
        }

        private void SaveCart(List<CartItemDto> cart) => Session.SetObject(CartKey, cart);

        public async Task AddAsync(int medicationId, int qty = 1)
        {
            if (qty <= 0) qty = 1;

            var med = await _db.Medications.AsNoTracking().FirstOrDefaultAsync(m => m.Id == medicationId);
            if (med == null) return;

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.MedicationId == medicationId);

            decimal price = GetMedicationPrice(med);

            if (existing == null)
            {
                cart.Add(new CartItemDto
                {
                    MedicationId = medicationId,
                    Name = med.Name ?? "Medication",
                    Price = price,
                    Quantity = qty
                });
            }
            else
            {
                existing.Quantity += qty;
            }

            SaveCart(cart);
        }

        public void UpdateQty(int medicationId, int qty)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MedicationId == medicationId);
            if (item == null) return;

            if (qty <= 0)
                cart.Remove(item);
            else
                item.Quantity = qty;

            SaveCart(cart);
        }

        public void Remove(int medicationId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.MedicationId == medicationId);
            SaveCart(cart);
        }

        public void Clear() => Session.Remove(CartKey);

        public decimal Total()
        {
            var cart = GetCart();
            return cart.Sum(x => x.Price * x.Quantity);
        }

        // ✅ adapte si ton Medication a un champ Price / UnitPrice
        private decimal GetMedicationPrice(Medication med)
        {
            // essaye plusieurs noms possibles (selon ton model)
            var prop = med.GetType().GetProperty("Price")
                       ?? med.GetType().GetProperty("UnitPrice")
                       ?? med.GetType().GetProperty("SalePrice");

            if (prop != null)
            {
                var val = prop.GetValue(med);
                if (val is decimal d) return d;
                if (val is double dd) return (decimal)dd;
            }
            return 0m;
        }
    }
}