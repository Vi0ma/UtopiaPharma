using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data; // Assurez-vous que ApplicationUser est accessible ici
using PharmaSys.Models;

namespace PharmaSys.Data
{
    public class PharmaSysDbContext : IdentityDbContext<ApplicationUser>
    {
        public PharmaSysDbContext(DbContextOptions<PharmaSysDbContext> options)
            : base(options) { }

        // ======= Existants
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Medication> Medications => Set<Medication>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();
        public DbSet<SupplierOrder> SupplierOrders => Set<SupplierOrder>();
        public DbSet<SupplierOrderItem> SupplierOrderItems => Set<SupplierOrderItem>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();

        // ======= Commandes Web (Espace Client)
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =======================
            // SupplierOrder
            // =======================
            builder.Entity<SupplierOrder>()
                .Property(x => x.OrderNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Entity<SupplierOrder>()
                .HasIndex(x => x.OrderNumber)
                .IsUnique();

            // =======================
            // SupplierOrderItem
            // =======================
            builder.Entity<SupplierOrderItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            // =======================
            // Prescription
            // =======================
            builder.Entity<Prescription>()
                .HasIndex(x => x.Number)
                .IsUnique();

            // =======================
            // Category
            // =======================
            builder.Entity<Category>()
                .HasIndex(x => x.Name)
                .IsUnique();

            // =======================
            // Client
            // =======================
            builder.Entity<Client>()
                .HasIndex(x => x.Code)
                .IsUnique();

            // =======================
            // Medication
            // =======================
            builder.Entity<Medication>()
                .HasIndex(x => x.Code)
                .IsUnique();

            // =======================
            // Sale / SaleItem
            // =======================
            builder.Entity<Sale>()
                .HasIndex(x => x.InvoiceNo)
                .IsUnique();

            builder.Entity<SaleItem>()
                .HasOne(x => x.Sale)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // =======================
            // Orders (Espace Client)
            // =======================
            builder.Entity<Order>()
                .Property(o => o.OrderNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // ✅ C'EST ICI LA CORRECTION : TotalAmount au lieu de Total
            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order!)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // =======================
            // OrderItem (Espace Client)
            // =======================
            builder.Entity<OrderItem>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(i => i.LineTotal)
                .HasPrecision(18, 2);
        }
    }
}