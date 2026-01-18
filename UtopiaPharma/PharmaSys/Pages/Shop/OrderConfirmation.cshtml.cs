using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace PharmaSys.Pages.Shop
{
    [AllowAnonymous] //  INDISPENSABLE : Permet à l'invité de voir cette page
    public class OrderConfirmationModel : PageModel
    {
        public string ClientName { get; set; } = "";
        public string ClientPhone { get; set; } = "";

        public void OnGet(string name, string phone)
        {
            ClientName = name;
            ClientPhone = phone;
        }
    }
}