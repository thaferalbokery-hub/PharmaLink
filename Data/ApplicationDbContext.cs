using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Models;

namespace PharmaLink.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pharmacy> Pharmacies { get; set; }
    public DbSet<Medicine> Medicines { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<SupplierMedicine> SupplierMedicines { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // === 1:1 RELATIONSHIP ===
        // ApplicationUser <-> Pharmacy (Owner)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.OwnedPharmacy)
            .WithOne(p => p.Owner)
            .HasForeignKey<Pharmacy>(p => p.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        // === 1:N RELATIONSHIPS ===

        // Pharmacy -> Inventory
        builder.Entity<Inventory>()
            .HasOne(i => i.Pharmacy)
            .WithMany(p => p.Inventories)
            .HasForeignKey(i => i.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Medicine -> Inventory
        builder.Entity<Inventory>()
            .HasOne(i => i.Medicine)
            .WithMany(m => m.Inventories)
            .HasForeignKey(i => i.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApplicationUser -> Prescriptions
        builder.Entity<Prescription>()
            .HasOne(p => p.User)
            .WithMany(u => u.Prescriptions)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prescription -> PrescriptionItems
        builder.Entity<PrescriptionItem>()
            .HasOne(pi => pi.Prescription)
            .WithMany(p => p.PrescriptionItems)
            .HasForeignKey(pi => pi.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Medicine -> PrescriptionItems
        builder.Entity<PrescriptionItem>()
            .HasOne(pi => pi.Medicine)
            .WithMany(m => m.PrescriptionItems)
            .HasForeignKey(pi => pi.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApplicationUser -> Sales
        builder.Entity<Sale>()
            .HasOne(s => s.User)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pharmacy -> Sales
        builder.Entity<Sale>()
            .HasOne(s => s.Pharmacy)
            .WithMany(p => p.Sales)
            .HasForeignKey(s => s.PharmacyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sale -> SaleItems
        builder.Entity<SaleItem>()
            .HasOne(si => si.Sale)
            .WithMany(s => s.SaleItems)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Medicine -> SaleItems
        builder.Entity<SaleItem>()
            .HasOne(si => si.Medicine)
            .WithMany(m => m.SaleItems)
            .HasForeignKey(si => si.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        // === M:N RELATIONSHIP (through SupplierMedicine) ===

        // Supplier -> SupplierMedicine
        builder.Entity<SupplierMedicine>()
            .HasOne(sm => sm.Supplier)
            .WithMany(s => s.SupplierMedicines)
            .HasForeignKey(sm => sm.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Medicine -> SupplierMedicine
        builder.Entity<SupplierMedicine>()
            .HasOne(sm => sm.Medicine)
            .WithMany(m => m.SupplierMedicines)
            .HasForeignKey(sm => sm.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);

        // === UNIQUE CONSTRAINTS ===
        builder.Entity<Inventory>()
            .HasIndex(i => new { i.PharmacyId, i.MedicineId })
            .IsUnique();

        builder.Entity<SupplierMedicine>()
            .HasIndex(sm => new { sm.SupplierId, sm.MedicineId })
            .IsUnique();

        // === INDEXES ===
        builder.Entity<Medicine>().HasIndex(m => m.Name);
        builder.Entity<Medicine>().HasIndex(m => m.Category);
        builder.Entity<Pharmacy>().HasIndex(p => p.City);
        builder.Entity<Supplier>().HasIndex(s => s.Name);
    }
}