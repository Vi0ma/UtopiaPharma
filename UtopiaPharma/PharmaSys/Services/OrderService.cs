using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Services
{
    public class OrderService
    {
        private readonly PharmaSysDbContext _db;
        private readonly CartService _cart;

        public OrderService(PharmaSysDbContext db, CartService cart)
        {
            _db = db;
            _cart = cart;
        }

        // Méthode pour le Checkout (Invités & Membres)
        public async Task CreateOrderAsync(Order order, List<CartItemDto> cartItems)
        {
            // 1. Générer le numéro de commande
            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                order.OrderNumber = $"CMD-{DateTime.Now:yyyyMMddHHmmss}";
            }

            // 2. Transformer le panier en items de commande
            foreach (var item in cartItems)
            {
                order.Items.Add(new OrderItem
                {
                    MedicationId = item.MedicationId,
                    Name = item.Name,
                    UnitPrice = item.Price,
                    Quantity = item.Quantity,
                    LineTotal = item.Price * item.Quantity
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
        }

        // Méthode optionnelle (si vous l'utilisez ailleurs)
        public async Task<Order?> CreateOrderFromCartAsync(string userId, PaymentMethod method)
        {
            var cart = _cart.GetCart();
            if (cart.Count == 0) return null;

            var order = new Order
            {
                UserId = userId, // On utilise UserId du modèle
                OrderNumber = $"CMD-{DateTime.Now:yyyyMMddHHmmss}",
                PaymentMethod = method,
                Status = "Paid", // ✅ Correction : String au lieu de Enum
                CreatedAt = DateTime.Now,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(i => i.Price * i.Quantity)
            };

            foreach (var c in cart)
            {
                order.Items.Add(new OrderItem
                {
                    MedicationId = c.MedicationId,
                    Name = c.Name,
                    UnitPrice = c.Price,
                    Quantity = c.Quantity,
                    LineTotal = c.Price * c.Quantity
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            _cart.Clear();
            return order;
        }

        public async Task<List<Order>> GetOrdersForUserAsync(string userId)
        {
            return await _db.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId) // Correction FK
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _db.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _db.Orders
                .Include(o => o.User) // Inclure le User pour l'affichage Admin
                .Include(o => o.Items)
                .ThenInclude(i => i.Medication)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                order.Status = status;
                await _db.SaveChangesAsync();
            }
        }
    }
}