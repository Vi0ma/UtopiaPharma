using PharmaSys.Data;
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Models
{
    public class Order
    {
        public int Id { get; set; }

        // ✅ On remet cette propriété indispensable pour le Service
        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ajouté pour compatibilité

        public decimal TotalAmount { get; set; }

        // On utilise string pour simplifier la gestion (Pending, Paid, Cancelled)
        public string Status { get; set; } = "Pending";

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CASH;

        // --- CHAMPS INVITÉ ---
        public string ClientName { get; set; } = "";
        public string ClientEmail { get; set; } = "";
        public string ClientPhone { get; set; } = "";
        public string ClientAddress { get; set; } = "";

        // --- CHAMPS MEMBRE (Optionnel) ---
        // On utilise UserId pour être standard avec Identity
        public string? UserId { get; set; }

        // Alias pour compatibilité si votre ancien code utilise "ClientUserId"
        public string? ClientUserId
        {
            get => UserId;
            set => UserId = value;
        }

        public ApplicationUser? User { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}