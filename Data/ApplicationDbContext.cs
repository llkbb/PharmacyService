using Microsoft.EntityFrameworkCore;
using PharmacyChain.Models;

namespace PharmacyChain.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
        public DbSet<Drug> Drugs => Set<Drug>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleLine> SaleLines => Set<SaleLine>();
        public DbSet<Employee> Employees => Set<Employee>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InventoryItem>()
                .HasIndex(i => new { i.PharmacyId, i.DrugId })
                .IsUnique();

            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(p => p.Lines)
                .WithOne(l => l.PurchaseOrder)
                .HasForeignKey(l => l.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sale>()
                .HasMany(s => s.Lines)
                .WithOne(l => l.Sale)
                .HasForeignKey(l => l.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
