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

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Pharmacy> Pharmacies { get; set; }
    public DbSet<PharmacyAddress> PharmacyAddresses { get; set; }
    public DbSet<Medicine> Medicines { get; set; }
    public DbSet<MedicineCategory> MedicineCategories { get; set; }
    public DbSet<MedicineBrand> MedicineBrands { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<MedicineImage> MedicineImages { get; set; }
    public DbSet<FavoritePharmacy> FavoritePharmacies { get; set; }
    public DbSet<FavoriteMedicine> FavoriteMedicines { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<PharmacyWorkingHour> PharmacyWorkingHours { get; set; }
    public DbSet<PharmacyContact> PharmacyContacts { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SearchHistory> SearchHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // === ONE-TO-ONE RELATIONSHIPS ===

        // ApplicationUser -> UserProfile (1:1)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pharmacy -> PharmacyAddress (1:1)
        builder.Entity<Pharmacy>()
            .HasOne(p => p.PharmacyAddress)
            .WithOne(a => a.Pharmacy)
            .HasForeignKey<PharmacyAddress>(a => a.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        // === ONE-TO-MANY RELATIONSHIPS ===

        // ApplicationUser -> Pharmacy (Owner)
        builder.Entity<Pharmacy>()
            .HasOne(p => p.Owner)
            .WithOne(u => u.OwnedPharmacy)
            .HasForeignKey<Pharmacy>(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pharmacy -> Inventory (1:N)
        builder.Entity<Inventory>()
            .HasOne(i => i.Pharmacy)
            .WithMany(p => p.Inventories)
            .HasForeignKey(i => i.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Medicine -> Inventory (1:N)
        builder.Entity<Inventory>()
            .HasOne(i => i.Medicine)
            .WithMany(m => m.Inventories)
            .HasForeignKey(i => i.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pharmacy -> WorkingHours (1:N)
        builder.Entity<PharmacyWorkingHour>()
            .HasOne(w => w.Pharmacy)
            .WithMany(p => p.WorkingHours)
            .HasForeignKey(w => w.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pharmacy -> Contacts (1:N)
        builder.Entity<PharmacyContact>()
            .HasOne(c => c.Pharmacy)
            .WithMany(p => p.Contacts)
            .HasForeignKey(c => c.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pharmacy -> Reviews (1:N)
        builder.Entity<Review>()
            .HasOne(r => r.Pharmacy)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApplicationUser -> Reviews (1:N)
        builder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // MedicineCategory -> Medicines (1:N)
        builder.Entity<Medicine>()
            .HasOne(m => m.Category)
            .WithMany(c => c.Medicines)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // MedicineBrand -> Medicines (1:N)
        builder.Entity<Medicine>()
            .HasOne(m => m.Brand)
            .WithMany(b => b.Medicines)
            .HasForeignKey(m => m.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        // Medicine -> Images (1:N)
        builder.Entity<MedicineImage>()
            .HasOne(i => i.Medicine)
            .WithMany(m => m.Images)
            .HasForeignKey(i => i.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);

        // Medicine -> InventoryTransactions (1:N)
        builder.Entity<InventoryTransaction>()
            .HasOne(t => t.Medicine)
            .WithMany(m => m.Transactions)
            .HasForeignKey(t => t.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pharmacy -> InventoryTransactions (1:N)
        builder.Entity<InventoryTransaction>()
            .HasOne(t => t.Pharmacy)
            .WithMany()
            .HasForeignKey(t => t.PharmacyId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApplicationUser -> Notifications (1:N)
        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApplicationUser -> SearchHistory (1:N)
        builder.Entity<SearchHistory>()
            .HasOne(s => s.User)
            .WithMany(u => u.SearchHistories)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // === MANY-TO-MANY RELATIONSHIPS ===

        // Customer <-> Pharmacy through FavoritePharmacy
        builder.Entity<FavoritePharmacy>()
            .HasOne(f => f.User)
            .WithMany(u => u.FavoritePharmacies)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FavoritePharmacy>()
            .HasOne(f => f.Pharmacy)
            .WithMany(p => p.FavoritedBy)
            .HasForeignKey(f => f.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Customer <-> Medicine through FavoriteMedicine
        builder.Entity<FavoriteMedicine>()
            .HasOne(f => f.User)
            .WithMany(u => u.FavoriteMedicines)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FavoriteMedicine>()
            .HasOne(f => f.Medicine)
            .WithMany(m => m.FavoritedBy)
            .HasForeignKey(f => f.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);

        // === UNIQUE CONSTRAINTS ===

        // Prevent duplicate Pharmacy+Medicine in Inventory
        builder.Entity<Inventory>()
            .HasIndex(i => new { i.PharmacyId, i.MedicineId })
            .IsUnique();

        // Prevent duplicate favorites
        builder.Entity<FavoritePharmacy>()
            .HasIndex(f => new { f.UserId, f.PharmacyId })
            .IsUnique();

        builder.Entity<FavoriteMedicine>()
            .HasIndex(f => new { f.UserId, f.MedicineId })
            .IsUnique();

        // Prevent duplicate working hours per day per pharmacy
        builder.Entity<PharmacyWorkingHour>()
            .HasIndex(w => new { w.PharmacyId, w.DayOfWeek })
            .IsUnique();

        // === INDEXES ===
        builder.Entity<Medicine>()
            .HasIndex(m => m.ScientificName);

        builder.Entity<Medicine>()
            .HasIndex(m => m.CommercialName);

        builder.Entity<Pharmacy>()
            .HasIndex(p => p.City);

        builder.Entity<Pharmacy>()
            .HasIndex(p => p.IsOpen);

        builder.Entity<Inventory>()
            .HasIndex(i => i.AvailabilityStatus);
    }
}