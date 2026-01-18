using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Pages.SupplierOrders
{
    public class CreateModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public CreateModel(PharmaSysDbContext db) => _db = db;

        public List<Supplier> Suppliers { get; set; } = new();
        public List<Medication> Medications { get; set; } = new();

        [BindProperty]
        public OrderInput Input { get; set; } = new();

        [BindProperty]
        public List<OrderItemInput> Items { get; set; } = new();

        public async Task OnGetAsync()
        {
            Suppliers = await _db.Suppliers.OrderBy(s => s.Name).ToListAsync();
            Medications = await _db.Medications.OrderBy(m => m.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Items.Count == 0)
            {
                ModelState.AddModelError("", "Le panier est vide.");
                await OnGetAsync();
                return Page();
            }

            // 1. Créer la commande
            var order = new SupplierOrder
            {
                SupplierId = Input.SupplierId,
                OrderDate = Input.OrderDate,
                OrderNumber = $"CMD-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}",
                Status = "Received",
                // Calcul du total avec UnitPrice
                TotalAmount = Items.Sum(x => x.Quantity * x.UnitPrice)
            };

            _db.SupplierOrders.Add(order);
            await _db.SaveChangesAsync();

            // 2. Ajouter les lignes
            foreach (var item in Items)
            {
                var orderItem = new SupplierOrderItem
                {
                    SupplierOrderId = order.Id,
                    MedicationId = item.MedicationId,
                    Quantity = item.Quantity,
                    // ✅ CORRECTION : On utilise UnitPrice ici
                    UnitPrice = item.UnitPrice
                };
                _db.SupplierOrderItems.Add(orderItem);

                // 3. Mettre à jour le stock
                var med = await _db.Medications.FindAsync(item.MedicationId);
                if (med != null)
                {
                    med.Stock += item.Quantity;
                    // Optionnel : Mettre à jour le prix d'achat du médicament aussi
                    // med.PurchasePrice = item.UnitPrice;
                }
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Commande enregistrée et stock mis à jour !";
            return RedirectToPage("Index");
        }

        public class OrderInput
        {
            [Required(ErrorMessage = "Le fournisseur est requis")]
            public int SupplierId { get; set; }
            public DateTime OrderDate { get; set; } = DateTime.Now;
        }

        public class OrderItemInput
        {
            public int MedicationId { get; set; }
            public int Quantity { get; set; }
            // ✅ CORRECTION : Nommé UnitPrice pour correspondre à la DB
            public decimal UnitPrice { get; set; }
        }
    }
}