using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL.Entities;

namespace SPIEMS.DAL;

public class SPIEMSDbContext : DbContext
{
    public SPIEMSDbContext(DbContextOptions<SPIEMSDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineBatch> MedicineBatches => Set<MedicineBatch>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierReturn> SupplierReturns => Set<SupplierReturn>();
    public DbSet<User> Users => Set<User>();

    // ✅ NEW
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

        // ✅ Sales relationships
        modelBuilder.Entity<Sale>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SaleLine>()
            .HasOne(l => l.Sale)
            .WithMany(s => s.Lines)
            .HasForeignKey(l => l.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SaleLine>()
            .HasOne(l => l.MedicineBatch)
            .WithMany()
            .HasForeignKey(l => l.MedicineBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // SEED USERS
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", Password = "admin123", Role = "Admin" },
            new User { Id = 2, Username = "staff", Password = "staff123", Role = "Staff" }
        );

        // SEED CATEGORIES
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Tablets", DefaultShelfLifeDays = 90 },
            new Category { Id = 2, Name = "Syrups", DefaultShelfLifeDays = 30 },
            new Category { Id = 3, Name = "Capsules", DefaultShelfLifeDays = 120 },
            new Category { Id = 4, Name = "Injections", DefaultShelfLifeDays = 60 }
        );

        // SEED MEDICINES
        modelBuilder.Entity<Medicine>().HasData(
            new Medicine { Id = 1, GenericName = "Paracetamol", BrandName = "Panadol", Company = "GSK", CategoryId = 1, ReorderLevel = 50 },
            new Medicine { Id = 2, GenericName = "Amoxicillin", BrandName = "Amoxil", Company = "Pfizer", CategoryId = 3, ReorderLevel = 30 },
            new Medicine { Id = 3, GenericName = "Ibuprofen", BrandName = "Brufen", Company = "Abbott", CategoryId = 1, ReorderLevel = 40 },
            new Medicine { Id = 4, GenericName = "Cough Syrup", BrandName = "Benylin", Company = "Johnson & Johnson", CategoryId = 2, ReorderLevel = 20 }
        );

        // SEED SUPPLIERS
        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, Name = "MedSupply Co", Phone = "021-1234567", Address = "Karachi", IsActive = true },
            new Supplier { Id = 2, Name = "PharmaDirect", Phone = "051-9876543", Address = "Islamabad", IsActive = true }
        );
    }
}