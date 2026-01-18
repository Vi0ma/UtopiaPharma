using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Medications
{
    public class CreateModel : PageModel
    {
        private readonly PharmaSysDbContext _db;

        public CreateModel(PharmaSysDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Medication Medication { get; set; } = new();

        public List<Category> Categories { get; set; } = new();
        public List<Supplier> Suppliers { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Charge les listes pour les menus déroulants
            Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
            Suppliers = await _db.Suppliers.OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ⚠️ IMPORTANT : Recharger les listes tout de suite
            // (Sinon, si l'enregistrement échoue, les menus déroulants deviennent vides)
            Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
            Suppliers = await _db.Suppliers.OrderBy(x => x.Name).ToListAsync();

            // 🔒 SÉCURITÉ 1 : Vérifier si le CODE existe déjà
            bool codeExists = await _db.Medications.AnyAsync(m => m.Code == Medication.Code);
            if (codeExists)
            {
                ModelState.AddModelError("Medication.Code", "Ce code produit existe déjà (doublon).");
            }

            // 🔒 SÉCURITÉ 2 : Vérifier si le CODE-BARRES existe déjà (s'il est renseigné)
            if (!string.IsNullOrEmpty(Medication.Barcode))
            {
                bool barcodeExists = await _db.Medications.AnyAsync(m => m.Barcode == Medication.Barcode);
                if (barcodeExists)
                {
                    ModelState.AddModelError("Medication.Barcode", "Ce code-barres est déjà utilisé par un autre médicament.");
                }
            }

            // Vérification standard (Champs obligatoires, etc.)
            if (!ModelState.IsValid)
            {
                return Page(); // On reste sur la page et on affiche les erreurs
            }

            // Tout est bon, on sauvegarde
            _db.Medications.Add(Medication);
            await _db.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}