using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaSys.Data;
using PharmaSys.Models;
using PharmaSys.Services; // Contient CartItemDto
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Pages.Shop
{
    [AllowAnonymous]
    public class CheckoutModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutModel(CartService cartService, OrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        // CORRECTION 1 : On utilise CartItemDto (celui défini dans CartService)
        public List<CartItemDto> CartItems { get; set; }
        public decimal Total { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Nom complet requis")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Email requis"), EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Téléphone requis")]
            public string Phone { get; set; }

            [Required(ErrorMessage = "Adresse requise")]
            public string Address { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // CORRECTION 2 : La méthode s'appelle GetCart()
            CartItems = _cartService.GetCart();

            if (!CartItems.Any()) return RedirectToPage("Cart");

            // CORRECTION 3 : CartItemDto n'a pas de propriété "Total", on calcule (Prix * Qté) ou on utilise _cartService.Total()
            Total = _cartService.Total();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    Input.Name = user.FullName ?? user.UserName;
                    Input.Email = user.Email;
                    // Input.Phone = user.PhoneNumber; 
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CartItems = _cartService.GetCart();
            if (!CartItems.Any()) return RedirectToPage("Cart");

            if (!ModelState.IsValid)
            {
                Total = _cartService.Total();
                return Page();
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                Status = "Pending", // Statut "En attente" pour que l'admin le voie
                TotalAmount = _cartService.Total(),

                ClientName = Input.Name,
                ClientEmail = Input.Email,
                ClientPhone = Input.Phone,
                ClientAddress = Input.Address,

                UserId = _userManager.GetUserId(User)
            };

            await _orderService.CreateOrderAsync(order, CartItems);
            _cartService.Clear();

            //  CORRECTION : Redirection vers la page de confirmation avec les infos
            return RedirectToPage("OrderConfirmation", new { name = Input.Name, phone = Input.Phone });
        }
    }
}