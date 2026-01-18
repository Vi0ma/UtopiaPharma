using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Sales
{
    public class DetailsModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public DetailsModel(PharmaSysDbContext db) => _db = db;

        public Sale Sale { get; set; } = null!;

        [BindProperty]
        public string NewStatus { get; set; }

        //  NOUVEAU : Pour récupérer le montant saisi
        [BindProperty]
        public decimal NewPaidAmount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var sale = await _db.Sales
                .Include(s => s.Client)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Medication)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound();

            Sale = sale;

            // On initialise le champ avec le montant déjà payé actuel
            NewPaidAmount = sale.Paid;

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id)
        {
            var sale = await _db.Sales.FindAsync(id);
            if (sale == null) return NotFound();

            if (!string.IsNullOrEmpty(NewStatus))
            {
                sale.Status = NewStatus;

                if (NewStatus == "Paid")
                {
                    // Automatique : Tout est payé
                    sale.Paid = sale.Total;
                }
                else if (NewStatus == "Unpaid")
                {
                    // Automatique : Rien n'est payé
                    sale.Paid = 0;
                }
                else if (NewStatus == "PartiallyPaid")
                {
                    //  MANUEL : On utilise le montant saisi par l'admin
                    // On s'assure qu'il ne dépasse pas le total
                    if (NewPaidAmount > sale.Total) sale.Paid = sale.Total;
                    else if (NewPaidAmount < 0) sale.Paid = 0;
                    else sale.Paid = NewPaidAmount;
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToPage(new { id });
        }
    }
}