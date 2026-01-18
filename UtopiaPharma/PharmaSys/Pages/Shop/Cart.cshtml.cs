using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaSys.Services;

namespace PharmaSys.Pages.Shop
{
    [AllowAnonymous]
    public class CartModel : PageModel
    {
        private readonly CartService _cart;
        public CartModel(CartService cart) { _cart = cart; }

        public List<CartItemDto> Items { get; set; } = new();
        public decimal Total { get; set; }

        public void OnGet()
        {
            Items = _cart.GetCart();
            Total = _cart.Total();
        }

        public IActionResult OnPostUpdate(int id, int qty)
        {
            _cart.UpdateQty(id, qty);
            return RedirectToPage();
        }

        public IActionResult OnPostRemove(int id)
        {
            _cart.Remove(id);
            return RedirectToPage();
        }
    }
}