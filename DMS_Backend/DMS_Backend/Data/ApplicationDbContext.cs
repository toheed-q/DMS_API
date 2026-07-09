using DMS.Models;
using Microsoft.EntityFrameworkCore;
using Route = DMS.Models.Route;   // disambiguate from Microsoft.AspNetCore.Routing.Route

namespace DMS_Backend.Data
{
    /// <summary>
    /// SQL Server database context for the DMS API. Entities and relationships
    /// mirror the desktop app so both share one schema — but this context is
    /// clean EF Core only (no SQLite-specific raw SQL/patches).
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Products> Products => Set<Products>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<DailyStockReport> DailyStockReports => Set<DailyStockReport>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Route> Routes => Set<Route>();
        public DbSet<Shop> Shops => Set<Shop>();
        public DbSet<Bill> Bills => Set<Bill>();
        public DbSet<BillItem> BillItems => Set<BillItem>();
        public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
        public DbSet<LedgerReceiving> LedgerReceivings => Set<LedgerReceiving>();
        public DbSet<Salesman> Salesmen => Set<Salesman>();
        public DbSet<DailySalesReport> DailySalesReports => Set<DailySalesReport>();
        public DbSet<DSRItem> DSRItems => Set<DSRItem>();
        public DbSet<DSRExpense> DSRExpenses => Set<DSRExpense>();
        public DbSet<StockIssuance> StockIssuances => Set<StockIssuance>();
        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<CompanyPaymentDetail> CompanyPaymentDetails => Set<CompanyPaymentDetail>();
        public DbSet<CompanyLedgerEntry> CompanyLedgerEntries => Set<CompanyLedgerEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Products - Category
            modelBuilder.Entity<Products>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Self-referencing Category
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Users
            modelBuilder.Entity<User>().ToTable("Users").HasKey(u => u.Id);
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            // Route - Shop
            modelBuilder.Entity<Shop>()
                .HasOne(s => s.Route)
                .WithMany(r => r.Shops)
                .HasForeignKey(s => s.RouteId)
                .OnDelete(DeleteBehavior.SetNull);

            // BillItem - Product / Bill
            // ProductsId is non-nullable, so SET NULL is invalid on SQL Server;
            // restrict instead (a product referenced by bill items can't be deleted).
            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Product)
                .WithMany()
                .HasForeignKey(bi => bi.ProductsId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Bill)
                .WithMany(b => b.BillItems)
                .HasForeignKey(bi => bi.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // LedgerEntry - Shop / Bill / Salesman
            modelBuilder.Entity<LedgerEntry>()
                .HasOne(l => l.Shop)
                .WithMany(s => s.LedgerEntries)
                .HasForeignKey(l => l.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LedgerEntry>()
                .HasOne(l => l.Bill)
                .WithMany()
                .HasForeignKey(l => l.BillId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LedgerEntry>()
                .HasOne(l => l.Salesman)
                .WithMany()
                .HasForeignKey(l => l.SalesmanId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LedgerEntry>().HasIndex(l => l.SalesmanId);

            // Salesman - DailySalesReport
            modelBuilder.Entity<DailySalesReport>()
                .HasOne(d => d.Salesman)
                .WithMany(s => s.DailySalesReports)
                .HasForeignKey(d => d.SalesmanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailySalesReport>()
                .HasOne(d => d.Route)
                .WithMany()
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.SetNull);

            // DSRItem - DSR / Product
            modelBuilder.Entity<DSRItem>()
                .HasOne(i => i.DailySalesReport)
                .WithMany(d => d.DSRItems)
                .HasForeignKey(i => i.DSRId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductId non-nullable -> restrict (SET NULL invalid on SQL Server).
            modelBuilder.Entity<DSRItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // StockIssuance - Salesman / Product
            modelBuilder.Entity<StockIssuance>()
                .HasOne(s => s.Salesman)
                .WithMany()
                .HasForeignKey(s => s.SalesmanId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductId non-nullable -> restrict (SET NULL invalid on SQL Server).
            modelBuilder.Entity<StockIssuance>()
                .HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // DSRExpense - DSR
            modelBuilder.Entity<DSRExpense>()
                .HasOne(e => e.DailySalesReport)
                .WithMany(d => d.DSRExpenses)
                .HasForeignKey(e => e.DSRId)
                .OnDelete(DeleteBehavior.Cascade);

            // Company - PaymentDetail / LedgerEntry
            modelBuilder.Entity<CompanyPaymentDetail>()
                .HasOne(p => p.Company)
                .WithMany(c => c.PaymentDetails)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyLedgerEntry>()
                .HasOne(l => l.Company)
                .WithMany(c => c.LedgerEntries)
                .HasForeignKey(l => l.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer-level receivings
            modelBuilder.Entity<LedgerReceiving>()
                .HasOne(r => r.Shop)
                .WithMany()
                .HasForeignKey(r => r.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LedgerReceiving>().HasIndex(r => r.ShopId);
            modelBuilder.Entity<LedgerReceiving>().HasIndex(r => new { r.ShopId, r.ReceivedDate });
        }
    }
}
