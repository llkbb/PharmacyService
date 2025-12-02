using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Models;

namespace PharmacyChain.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
        public DbSet<Drug> Drugs => Set<Drug>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Employee> Employees => Set<Employee>();

        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleLine> SaleLines => Set<SaleLine>();

        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // InventoryItem повинні бути унікальні по Drug + Pharmacy
            builder.Entity<InventoryItem>()
                .HasIndex(i => new { i.PharmacyId, i.DrugId })
                .IsUnique();

            // Sale → SaleLine (1-to-many)
            builder.Entity<Sale>()
                .HasMany(s => s.Lines)
                .WithOne(l => l.Sale)
                .HasForeignKey(l => l.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // PurchaseOrder → Lines
            builder.Entity<PurchaseOrder>()
                .HasMany(p => p.Lines)
                .WithOne(l => l.PurchaseOrder)
                .HasForeignKey(l => l.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
