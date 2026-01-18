using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Pages.Medications
{
    [Authorize(Roles = "Admin,Pharmacien")]
    public class EditModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public EditModel(PharmaSysDbContext db) => _db = db;

        public List<Category> Categories { get; set; } = new();
        public List<Supplier> Suppliers { get; set; } = new();

        [BindProperty] public InputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return RedirectToPage("/Medications/Index");

            Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
            Suppliers = await _db.Suppliers.OrderBy(x => x.Name).ToListAsync();

            var med = await _db.Medications.FirstOrDefaultAsync(x => x.Id == id);
            if (med == null) return RedirectToPage("/Medications/Index");

            Input = new InputModel
            {
                Id = med.Id,
                Code = med.Code,
                Name = med.Name,
                Dosage = med.Dosage,
                Manufacturer = med.Manufacturer,
                Barcode = med.Barcode,
                CategoryId = med.CategoryId,
                SupplierId = med.SupplierId,
                Stock = med.Stock,
                StockMin = med.StockMin,
                PurchasePrice = med.PurchasePrice,
                SalePrice = med.SalePrice,
                ExpiryDate = med.ExpirationDate,
                Location = med.Location
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // IMPORTANT: recharger listes si on revient sur la page (validation error)
            Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
            Suppliers = await _db.Suppliers.OrderBy(x => x.Name).ToListAsync();

            // 🔒 SÉCURITÉ 1 : Vérifier si le CODE existe déjà (sur un AUTRE médicament)
            // Note: On ajoute "&& m.Id != Input.Id" pour ne pas se bloquer soi-même
            bool codeExists = await _db.Medications.AnyAsync(m => m.Code == Input.Code && m.Id != Input.Id);
            if (codeExists)
            {
                ModelState.AddModelError("Input.Code", "Ce code produit est déjà utilisé par un autre médicament.");
            }

            // 🔒 SÉCURITÉ 2 : Vérifier si le CODE-BARRES existe déjà (sur un AUTRE médicament)
            if (!string.IsNullOrEmpty(Input.Barcode))
            {
                bool barcodeExists = await _db.Medications.AnyAsync(m => m.Barcode == Input.Barcode && m.Id != Input.Id);
                if (barcodeExists)
                {
                    ModelState.AddModelError("Input.Barcode", "Ce code-barres est déjà utilisé par un autre médicament.");
                }
            }

            if (!ModelState.IsValid) return Page();

            var med = await _db.Medications.FirstOrDefaultAsync(x => x.Id == Input.Id);
            if (med == null) return RedirectToPage("/Medications/Index");

            med.Code = Input.Code.Trim();
            med.Name = Input.Name.Trim();
            med.Dosage = Input.Dosage?.Trim();
            med.Manufacturer = Input.Manufacturer?.Trim();
            med.Barcode = Input.Barcode?.Trim();
            med.CategoryId = Input.CategoryId;
            med.SupplierId = Input.SupplierId;

            med.Stock = Input.Stock;
            med.StockMin = Input.StockMin;
            med.PurchasePrice = Input.PurchasePrice;
            med.SalePrice = Input.SalePrice;
            med.ExpirationDate = Input.ExpiryDate;
            med.Location = Input.Location?.Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] = "Médicament mis à jour avec succès.";
            return RedirectToPage("/Medications/Index");
        }

        public class InputModel
        {
            public int Id { get; set; }

            [Required] public string Code { get; set; } = "";
            [Required] public string Name { get; set; } = "";

            public string? Dosage { get; set; }
            public string? Manufacturer { get; set; }
            public string? Barcode { get; set; }

            public int? CategoryId { get; set; }
            public int? SupplierId { get; set; }

            [Range(0, 1000000, ErrorMessage = "Le stock ne peut pas être négatif")]
            public int Stock { get; set; } = 0;

            [Range(0, 1000000, ErrorMessage = "Le stock min ne peut pas être négatif")]
            public int StockMin { get; set; } = 0;

            [Range(0, double.MaxValue, ErrorMessage = "Le prix d'achat ne peut pas être négatif")]
            public decimal PurchasePrice { get; set; } = 0;

            [Range(0, double.MaxValue, ErrorMessage = "Le prix de vente ne peut pas être négatif")]
            public decimal SalePrice { get; set; } = 0;

            public DateTime? ExpiryDate { get; set; }
            public string? Location { get; set; }
        }
    }
}