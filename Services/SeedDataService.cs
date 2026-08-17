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

    public SeedDataService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
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
        await SeedCategoriesAsync();
        await SeedBrandsAsync();
        await SeedMedicinesAsync();
        await SeedPharmaciesAsync();
        await SeedInventoryAsync();
        await SeedWorkingHoursAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = { "Admin", "Pharmacist", "Customer" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        // Admin
        if (await _userManager.FindByEmailAsync("admin@pharmalink.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@pharmalink.com",
                Email = "admin@pharmalink.com",
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(admin, "Admin");
        }

        // Pharmacist
        if (await _userManager.FindByEmailAsync("pharmacist@pharmalink.com") == null)
        {
            var pharmacist = new ApplicationUser
            {
                UserName = "pharmacist@pharmalink.com",
                Email = "pharmacist@pharmalink.com",
                FirstName = "Ahmed",
                LastName = "Al-Pharmacy",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(pharmacist, "Pharm@123");
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(pharmacist, "Pharmacist");
        }

        // Second Pharmacist
        if (await _userManager.FindByEmailAsync("pharmacist2@pharmalink.com") == null)
        {
            var pharmacist2 = new ApplicationUser
            {
                UserName = "pharmacist2@pharmalink.com",
                Email = "pharmacist2@pharmalink.com",
                FirstName = "Sara",
                LastName = "Al-Dawaa",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(pharmacist2, "Pharm@123");
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(pharmacist2, "Pharmacist");
        }

        // Customer
        if (await _userManager.FindByEmailAsync("customer@pharmalink.com") == null)
        {
            var customer = new ApplicationUser
            {
                UserName = "customer@pharmalink.com",
                Email = "customer@pharmalink.com",
                FirstName = "Mohammed",
                LastName = "Customer",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(customer, "Cust@123");
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(customer, "Customer");
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.MedicineCategories.AnyAsync()) return;

        var categories = new List<MedicineCategory>
        {
            new() { Name = "Pain Relief", Description = "Medications for pain management" },
            new() { Name = "Vitamins & Supplements", Description = "Vitamins, minerals, and dietary supplements" },
            new() { Name = "Medical Supplies", Description = "Medical devices and supplies" },
            new() { Name = "First Aid", Description = "First aid supplies and medications" },
            new() { Name = "Personal Care", Description = "Personal hygiene and care products" },
            new() { Name = "Baby Care", Description = "Baby health and care products" },
            new() { Name = "Hygiene", Description = "Hygiene and sanitation products" },
            new() { Name = "Antibiotics", Description = "Antibiotic medications" },
            new() { Name = "Cardiovascular", Description = "Heart and blood pressure medications" },
            new() { Name = "Diabetes", Description = "Diabetes management medications" },
            new() { Name = "Respiratory", Description = "Respiratory and allergy medications" },
            new() { Name = "Digestive", Description = "Digestive system medications" }
        };

        _context.MedicineCategories.AddRange(categories);
        await _context.SaveChangesAsync();
    }

    private async Task SeedBrandsAsync()
    {
        if (await _context.MedicineBrands.AnyAsync()) return;

        var brands = new List<MedicineBrand>
        {
            new() { Name = "Pfizer", Description = "Global pharmaceutical company" },
            new() { Name = "Novartis", Description = "Swiss multinational pharmaceutical company" },
            new() { Name = "Roche", Description = "Swiss healthcare company" },
            new() { Name = "Johnson & Johnson", Description = "American healthcare company" },
            new() { Name = "Sanofi", Description = "French multinational pharmaceutical company" },
            new() { Name = "GSK", Description = "British pharmaceutical company" },
            new() { Name = "AstraZeneca", Description = "British-Swedish pharmaceutical company" },
            new() { Name = "Bayer", Description = "German pharmaceutical company" },
            new() { Name = "SPIMACO", Description = "Saudi pharmaceutical company" },
            new() { Name = "Tabuk Pharmaceutical", Description = "Saudi pharmaceutical company" }
        };

        _context.MedicineBrands.AddRange(brands);
        await _context.SaveChangesAsync();
    }

    private async Task SeedMedicinesAsync()
    {
        if (await _context.Medicines.AnyAsync()) return;

        var painRelief = await _context.MedicineCategories.FirstAsync(c => c.Name == "Pain Relief");
        var vitamins = await _context.MedicineCategories.FirstAsync(c => c.Name == "Vitamins & Supplements");
        var antibiotics = await _context.MedicineCategories.FirstAsync(c => c.Name == "Antibiotics");
        var cardiovascular = await _context.MedicineCategories.FirstAsync(c => c.Name == "Cardiovascular");
        var diabetes = await _context.MedicineCategories.FirstAsync(c => c.Name == "Diabetes");
        var respiratory = await _context.MedicineCategories.FirstAsync(c => c.Name == "Respiratory");
        var digestive = await _context.MedicineCategories.FirstAsync(c => c.Name == "Digestive");

        var pfizer = await _context.MedicineBrands.FirstAsync(b => b.Name == "Pfizer");
        var novartis = await _context.MedicineBrands.FirstAsync(b => b.Name == "Novartis");
        var gsk = await _context.MedicineBrands.FirstAsync(b => b.Name == "GSK");
        var bayer = await _context.MedicineBrands.FirstAsync(b => b.Name == "Bayer");
        var sanofi = await _context.MedicineBrands.FirstAsync(b => b.Name == "Sanofi");

        var medicines = new List<Medicine>
        {
            new() { ScientificName = "Paracetamol", CommercialName = "Panadol", Description = "Used for pain relief and fever reduction", CategoryId = painRelief.Id, BrandId = gsk.Id, DosageForm = "Tablet", Strength = "500mg", Unit = "mg" },
            new() { ScientificName = "Ibuprofen", CommercialName = "Advil", Description = "Non-steroidal anti-inflammatory drug for pain and inflammation", CategoryId = painRelief.Id, BrandId = pfizer.Id, DosageForm = "Tablet", Strength = "400mg", Unit = "mg" },
            new() { ScientificName = "Amoxicillin", CommercialName = "Amoxil", Description = "Broad-spectrum antibiotic", CategoryId = antibiotics.Id, BrandId = gsk.Id, DosageForm = "Capsule", Strength = "500mg", Unit = "mg", RequiresPrescription = true },
            new() { ScientificName = "Metformin", CommercialName = "Glucophage", Description = "Oral diabetes medicine that helps control blood sugar levels", CategoryId = diabetes.Id, BrandId = sanofi.Id, DosageForm = "Tablet", Strength = "850mg", Unit = "mg", RequiresPrescription = true },
            new() { ScientificName = "Amlodipine", CommercialName = "Norvasc", Description = "Calcium channel blocker for high blood pressure", CategoryId = cardiovascular.Id, BrandId = pfizer.Id, DosageForm = "Tablet", Strength = "5mg", Unit = "mg", RequiresPrescription = true },
            new() { ScientificName = "Omeprazole", CommercialName = "Losec", Description = "Proton pump inhibitor for acid reflux", CategoryId = digestive.Id, BrandId = novartis.Id, DosageForm = "Capsule", Strength = "20mg", Unit = "mg" },
            new() { ScientificName = "Cetirizine", CommercialName = "Zyrtec", Description = "Antihistamine for allergy relief", CategoryId = respiratory.Id, BrandId = pfizer.Id, DosageForm = "Tablet", Strength = "10mg", Unit = "mg" },
            new() { ScientificName = "Vitamin D3", CommercialName = "Vi-De 3", Description = "Vitamin D supplement for bone health", CategoryId = vitamins.Id, BrandId = novartis.Id, DosageForm = "Drops", Strength = "4500IU", Unit = "IU/ml" },
            new() { ScientificName = "Vitamin C", CommercialName = "Cevitil", Description = "Vitamin C supplement for immune support", CategoryId = vitamins.Id, BrandId = bayer.Id, DosageForm = "Effervescent Tablet", Strength = "1000mg", Unit = "mg" },
            new() { ScientificName = "Aspirin", CommercialName = "Aspirin Protect", Description = "Blood thinner and pain reliever", CategoryId = cardiovascular.Id, BrandId = bayer.Id, DosageForm = "Tablet", Strength = "100mg", Unit = "mg" },
            new() { ScientificName = "Salbutamol", CommercialName = "Ventolin", Description = "Bronchodilator for asthma relief", CategoryId = respiratory.Id, BrandId = gsk.Id, DosageForm = "Inhaler", Strength = "100mcg", Unit = "mcg", RequiresPrescription = true },
            new() { ScientificName = "Diclofenac", CommercialName = "Voltaren", Description = "Anti-inflammatory for pain and swelling", CategoryId = painRelief.Id, BrandId = novartis.Id, DosageForm = "Gel", Strength = "1%", Unit = "%" }
        };

        _context.Medicines.AddRange(medicines);
        await _context.SaveChangesAsync();
    }

    private async Task SeedPharmaciesAsync()
    {
        if (await _context.Pharmacies.AnyAsync()) return;

        var pharmacist = await _userManager.FindByEmailAsync("pharmacist@pharmalink.com");
        var pharmacist2 = await _userManager.FindByEmailAsync("pharmacist2@pharmalink.com");

        if (pharmacist == null || pharmacist2 == null) return;

        var pharmacies = new List<Pharmacy>
        {
            new()
            {
                Name = "Al-Dawaa Pharmacy",
                Description = "Leading pharmacy chain providing quality healthcare products and services",
                Phone = "+966501234567",
                Email = "aldawaa@pharmalink.com",
                Address = "King Fahd Road, Al-Olaya District",
                City = "Riyadh",
                Latitude = 24.7136,
                Longitude = 46.6753,
                IsOpen = true,
                IsActive = true,
                OwnerId = pharmacist.Id
            },
            new()
            {
                Name = "Al-Nahdi Pharmacy",
                Description = "Trusted pharmacy with wide range of medicines and health products",
                Phone = "+966509876543",
                Email = "alnahdi@pharmalink.com",
                Address = "Prince Sultan Road, Al-Zahra District",
                City = "Jeddah",
                Latitude = 21.5433,
                Longitude = 39.1728,
                IsOpen = true,
                IsActive = true,
                OwnerId = pharmacist2.Id
            }
        };

        _context.Pharmacies.AddRange(pharmacies);
        await _context.SaveChangesAsync();
    }

    private async Task SeedInventoryAsync()
    {
        if (await _context.Inventories.AnyAsync()) return;

        var pharmacies = await _context.Pharmacies.ToListAsync();
        var medicines = await _context.Medicines.ToListAsync();

        if (!pharmacies.Any() || !medicines.Any()) return;

        var random = new Random(42);
        var inventories = new List<Inventory>();

        foreach (var pharmacy in pharmacies)
        {
            foreach (var medicine in medicines)
            {
                var quantity = random.Next(0, 100);
                var price = Math.Round((decimal)(random.NextDouble() * 50 + 5), 2);
                var status = quantity == 0 ? AvailabilityStatus.OutOfStock
                    : quantity <= 10 ? AvailabilityStatus.LowStock
                    : AvailabilityStatus.Available;

                inventories.Add(new Inventory
                {
                    PharmacyId = pharmacy.Id,
                    MedicineId = medicine.Id,
                    Quantity = quantity,
                    Price = price,
                    AvailabilityStatus = status,
                    LastUpdated = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                });
            }
        }

        _context.Inventories.AddRange(inventories);
        await _context.SaveChangesAsync();
    }

    private async Task SeedWorkingHoursAsync()
    {
        if (await _context.PharmacyWorkingHours.AnyAsync()) return;

        var pharmacies = await _context.Pharmacies.ToListAsync();
        var workingHours = new List<PharmacyWorkingHour>();

        foreach (var pharmacy in pharmacies)
        {
            for (int day = 0; day < 7; day++)
            {
                var isFriday = (DayOfWeek)day == DayOfWeek.Friday;
                workingHours.Add(new PharmacyWorkingHour
                {
                    PharmacyId = pharmacy.Id,
                    DayOfWeek = (DayOfWeek)day,
                    OpeningTime = isFriday ? new TimeSpan(16, 0, 0) : new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(23, 0, 0),
                    IsClosed = false
                });
            }
        }

        _context.PharmacyWorkingHours.AddRange(workingHours);
        await _context.SaveChangesAsync();
    }
}