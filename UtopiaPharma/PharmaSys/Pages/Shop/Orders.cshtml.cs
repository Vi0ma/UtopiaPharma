using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaSys.Data;
using PharmaSys.Models;
using PharmaSys.Services;

namespace PharmaSys.Pages.Shop
{
    [Authorize]
    public class OrdersModel : PageModel
    {
        private readonly OrderService _orders;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersModel(OrderService orders, UserManager<ApplicationUser> userManager)
        {
            _orders = orders;
            _userManager = userManager;
        }

        public List<Order> Orders { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;
            Orders = await _orders.GetOrdersForUserAsync(user.Id);
        }
    }
}