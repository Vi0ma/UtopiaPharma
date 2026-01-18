using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // N'oubliez pas ce using
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public CreateModel(PharmaSysDbContext db) => _db = db;

        [BindProperty] public Client Client { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            //  SÉCURITÉ : Code unique
            bool codeExists = await _db.Clients.AnyAsync(c => c.Code == Client.Code);
            if (codeExists)
            {
                ModelState.AddModelError("Client.Code", "Ce code client existe déjà.");
            }

            if (!ModelState.IsValid) return Page();

            _db.Clients.Add(Client);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}