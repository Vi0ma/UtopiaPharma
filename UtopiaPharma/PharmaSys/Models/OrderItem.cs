using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int MedicationId { get; set; }

        
        public Medication? Medication { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }
    }
}