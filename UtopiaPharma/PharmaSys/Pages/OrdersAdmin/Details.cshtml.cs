using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaSys.Models;
using PharmaSys.Services;

namespace PharmaSys.Pages.OrdersAdmin
{
    // Sécurisé : Seul un admin ou pharmacien peut voir/valider
    [Authorize(Roles = "Admin,Pharmacien")]
    public class DetailsModel : PageModel
    {
        private readonly OrderService _orderService;

        public DetailsModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        public Order Order { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _orderService.GetByIdAsync(id);

            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }

        //  ACTION : VALIDER OU ANNULER LA COMMANDE
        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            await _orderService.UpdateStatusAsync(id, status);

            // On recharge la page pour voir le changement
            return RedirectToPage(new { id });
        }
    }
}