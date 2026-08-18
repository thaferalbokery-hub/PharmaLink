using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Services;

public class SeedDataService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SeedDataService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();
        await SeedRolesAsync();
        await SeedUsersAsync();
        await SeedPharmaciesAsync();
        await SeedMedicinesAsync();
        await SeedSuppliersAsync();
        await SeedInventoryAsync();
        await SeedSupplierMedicinesAsync();
        await SeedPrescriptionsAsync();
        await SeedSalesAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = { "Admin", "Pharmacist", "Customer" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _userManager.FindByEmailAsync("admin@pharmalink.com") == null)
        {
            var admin = new ApplicationUser { UserName = "admin@pharmalink.com", Email = "admin@pharmalink.com", FirstName = "System", LastName = "Admin", EmailConfirmed = true, IsActive = true };
            await _userManager.CreateAsync(admin, "Admin@123");
            await _userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await _userManager.FindByEmailAsync("pharmacist@pharmalink.com") == null)
        {
            var pharmacist = new ApplicationUser { UserName = "pharmacist@pharmalink.com", Email = "pharmacist@pharmalink.com", FirstName = "Ahmed", LastName = "Pharmacist", EmailConfirmed = true, IsActive = true };
            await _userManager.CreateAsync(pharmacist, "Pharm@123");
            await _userManager.AddToRoleAsync(pharmacist, "Pharmacist");
        }

        if (await _userManager.FindByEmailAsync("customer@pharmalink.com") == null)
        {
            var customer = new ApplicationUser { UserName = "customer@pharmalink.com", Email = "customer@pharmalink.com", FirstName = "Mohammed", LastName = "Customer", EmailConfirmed = true, IsActive = true };
            await _userManager.CreateAsync(customer, "Cust@123");
            await _userManager.AddToRoleAsync(customer, "Customer");
        }
    }

    private async Task SeedPharmaciesAsync()
    {
        if (await _context.Pharmacies.AnyAsync()) return;

        var pharmacist = await _userManager.FindByEmailAsync("pharmacist@pharmalink.com");

        var pharmacies = new List<Pharmacy>
        {
            new() { Name = "Al-Dawaa Pharmacy", Description = "Leading pharmacy chain", Phone = "+966501234567", Email = "aldawaa@pharmalink.com", Address = "King Fahd Road", City = "Riyadh", IsOpen = true, OwnerId = pharmacist?.Id },
            new() { Name = "Al-Nahdi Pharmacy", Description = "Trusted pharmacy", Phone = "+966509876543", Email = "alnahdi@pharmalink.com", Address = "Prince Sultan Road", City = "Jeddah", IsOpen = true },
            new() { Name = "Care Pharmacy", Description = "Your health partner", Phone = "+966551112233", Email = "care@pharmalink.com", Address = "Olaya Street", City = "Riyadh", IsOpen = false }
        };

        _context.Pharmacies.AddRange(pharmacies);
        await _context.SaveChangesAsync();
    }

    private async Task SeedMedicinesAsync()
    {
        if (await _context.Medicines.AnyAsync()) return;

        var medicines = new List<Medicine>
        {
            new() { Name = "Panadol 500mg", Description = "Pain relief and fever reduction", Category = "Pain Relief", Price = 12.50m, Quantity = 500, ExpiryDate = DateTime.UtcNow.AddYears(2) },
            new() { Name = "Amoxicillin 500mg", Description = "Broad-spectrum antibiotic", Category = "Antibiotics", Price = 25.00m, Quantity = 200, RequiresPrescription = true, ExpiryDate = DateTime.UtcNow.AddYears(1) },
            new() { Name = "Vitamin D3 1000IU", Description = "Vitamin D supplement", Category = "Vitamins", Price = 35.00m, Quantity = 300, ExpiryDate = DateTime.UtcNow.AddYears(3) },
            new() { Name = "Metformin 850mg", Description = "Diabetes management", Category = "Diabetes", Price = 18.00m, Quantity = 150, RequiresPrescription = true, ExpiryDate = DateTime.UtcNow.AddMonths(18) },
            new() { Name = "Omeprazole 20mg", Description = "Acid reflux treatment", Category = "Digestive", Price = 22.00m, Quantity = 250, ExpiryDate = DateTime.UtcNow.AddYears(2) },
            new() { Name = "Cetirizine 10mg", Description = "Allergy relief", Category = "Allergy", Price = 15.00m, Quantity = 400, ExpiryDate = DateTime.UtcNow.AddYears(2) },
            new() { Name = "Aspirin 100mg", Description = "Blood thinner", Category = "Cardiovascular", Price = 8.50m, Quantity = 600, ExpiryDate = DateTime.UtcNow.AddYears(3) },
            new() { Name = "Ibuprofen 400mg", Description = "Anti-inflammatory", Category = "Pain Relief", Price = 14.00m, Quantity = 350, ExpiryDate = DateTime.UtcNow.AddYears(2) },
            new() { Name = "Ventolin Inhaler", Description = "Asthma relief", Category = "Respiratory", Price = 45.00m, Quantity = 80, RequiresPrescription = true, ExpiryDate = DateTime.UtcNow.AddYears(1) },
            new() { Name = "Vitamin C 1000mg", Description = "Immune support", Category = "Vitamins", Price = 20.00m, Quantity = 500, ExpiryDate = DateTime.UtcNow.AddYears(3) }
        };

        _context.Medicines.AddRange(medicines);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSuppliersAsync()
    {
        if (await _context.Suppliers.AnyAsync()) return;

        var suppliers = new List<Supplier>
        {
            new() { Name = "Pfizer Saudi", ContactPerson = "Ali Hassan", Phone = "+966501111111", Email = "pfizer@supplier.com", Address = "Industrial Area, Riyadh" },
            new() { Name = "Novartis Gulf", ContactPerson = "Sara Ahmed", Phone = "+966502222222", Email = "novartis@supplier.com", Address = "Business District, Jeddah" },
            new() { Name = "SPIMACO", ContactPerson = "Khalid Omar", Phone = "+966503333333", Email = "spimaco@supplier.com", Address = "Pharmaceutical Zone, Dammam" }
        };

        _context.Suppliers.AddRange(suppliers);
        await _context.SaveChangesAsync();
    }

    private async Task SeedInventoryAsync()
    {
        if (await _context.Inventories.AnyAsync()) return;

        var pharmacies = await _context.Pharmacies.ToListAsync();
        var medicines = await _context.Medicines.ToListAsync();
        var random = new Random(42);

        foreach (var pharmacy in pharmacies)
        {
            foreach (var medicine in medicines.Take(7))
            {
                _context.Inventories.Add(new Inventory
                {
                    PharmacyId = pharmacy.Id,
                    MedicineId = medicine.Id,
                    Quantity = random.Next(0, 100),
                    MinimumStockLevel = 10,
                    LastUpdated = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                });
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedSupplierMedicinesAsync()
    {
        if (await _context.SupplierMedicines.AnyAsync()) return;

        var suppliers = await _context.Suppliers.ToListAsync();
        var medicines = await _context.Medicines.ToListAsync();
        var random = new Random(42);

        foreach (var supplier in suppliers)
        {
            foreach (var medicine in medicines.Take(5))
            {
                _context.SupplierMedicines.Add(new SupplierMedicine
                {
                    SupplierId = supplier.Id,
                    MedicineId = medicine.Id,
                    SupplyPrice = medicine.Price * 0.7m,
                    AvailableQuantity = random.Next(50, 500),
                    LastSupplyDate = DateTime.UtcNow.AddDays(-random.Next(1, 60))
                });
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedPrescriptionsAsync()
    {
        if (await _context.Prescriptions.AnyAsync()) return;

        var customer = await _userManager.FindByEmailAsync("customer@pharmalink.com");
        if (customer == null) return;

        var medicines = await _context.Medicines.Where(m => m.RequiresPrescription).ToListAsync();

        var prescription = new Prescription
        {
            UserId = customer.Id,
            PrescriptionDate = DateTime.UtcNow.AddDays(-5),
            Status = PrescriptionStatus.Approved,
            DoctorName = "Dr. Ahmad",
            Notes = "Take after meals"
        };
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        foreach (var med in medicines.Take(2))
        {
            _context.PrescriptionItems.Add(new PrescriptionItem
            {
                PrescriptionId = prescription.Id,
                MedicineId = med.Id,
                Quantity = 1,
                DosageInstructions = "1 tablet twice daily"
            });
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedSalesAsync()
    {
        if (await _context.Sales.AnyAsync()) return;

        var customer = await _userManager.FindByEmailAsync("customer@pharmalink.com");
        var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync();
        if (customer == null || pharmacy == null) return;

        var medicines = await _context.Medicines.Take(3).ToListAsync();

        var sale = new Sale
        {
            UserId = customer.Id,
            PharmacyId = pharmacy.Id,
            SaleDate = DateTime.UtcNow.AddDays(-2),
            TotalAmount = 0,
            Status = SaleStatus.Completed
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        decimal total = 0;
        foreach (var med in medicines)
        {
            var qty = 2;
            _context.SaleItems.Add(new SaleItem
            {
                SaleId = sale.Id,
                MedicineId = med.Id,
                Quantity = qty,
                UnitPrice = med.Price
            });
            total += med.Price * qty;
        }
        sale.TotalAmount = total;
        await _context.SaveChangesAsync();
    }
}