using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Medications
{
    public class DeleteModel : PageModel
    {
        private readonly PharmaSysDbContext _context;

        public DeleteModel(PharmaSysDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Medication Medication { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var medication = await _context.Medications
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null) return NotFound();

            Medication = medication;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var medication = await _context.Medications.FindAsync(id);
            if (medication == null) return NotFound();

            // 1. Vérifier si le médicament a un historique
            bool hasHistory = await _context.SaleItems.AnyAsync(x => x.MedicationId == id)
                           || await _context.OrderItems.AnyAsync(x => x.MedicationId == id)
                           || await _context.SupplierOrderItems.AnyAsync(x => x.MedicationId == id);

            if (hasHistory)
            {
                //  SCÉNARIO 1 : Il a un historique -> ON ARCHIVE (Désactivation)
                // Cela permet de garder les factures intactes tout en "supprimant" le produit de la vue.
                medication.IsActive = false;

                // Optionnel : On peut vider le stock pour éviter qu'il ressorte
                medication.Stock = 0;

                _context.Attach(medication).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            else
            {
                //  SCÉNARIO 2 : Aucun historique -> SUPPRESSION TOTALE
                _context.Medications.Remove(medication);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}