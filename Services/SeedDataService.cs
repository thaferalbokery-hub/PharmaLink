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
        // Admin user
        if (await _userManager.FindByEmailAsync("admin@pharmalink.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@pharmalink.com",
                Email = "admin@pharmalink.com",
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                City = "Riyadh",
                Address = "King Fahd Road, Building 5"
            };
            var result = await _userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(admin, "Admin");
        }

        // Pharmacist user
        if (await _userManager.FindByEmailAsync("pharmacist@pharmalink.com") == null)
        {
            var pharmacist = new ApplicationUser
            {
                UserName = "pharmacist@pharmalink.com",
                Email = "pharmacist@pharmalink.com",
                FirstName = "Ahmed",
                LastName = "Al-Farsi",
                EmailConfirmed = true,
                IsActive = true,
                City = "Riyadh",
                Address = "Olaya Street, Suite 12"
            };
            var result = await _userManager.CreateAsync(pharmacist, "Pharm@123");
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(pharmacist, "Pharmacist");
        }

        // Customer user
        if (await _userManager.FindByEmailAsync("customer@pharmalink.com") == null)
        {
            var customer = new ApplicationUser
            {
                UserName = "customer@pharmalink.com",
                Email = "customer@pharmalink.com",
                FirstName = "Mohammed",
                LastName = "Al-Rashid",
                EmailConfirmed = true,
                IsActive = true,
                City = "Jeddah",
                Address = "Prince Sultan Road"
            };
            var result = await _userManager.CreateAsync(customer, "Cust@123");
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(customer, "Customer");
        }
    }

    private async Task SeedPharmaciesAsync()
    {
        if (await _context.Pharmacies.AnyAsync()) return;

        var pharmacist = await _userManager.FindByEmailAsync("pharmacist@pharmalink.com");

        var pharmacies = new List<Pharmacy>
        {
            new()
            {
                Name = "Al-Dawaa Pharmacy",
                Description = "Leading pharmacy chain providing comprehensive pharmaceutical services with a wide range of medicines and health products.",
                Phone = "+966501234567",
                Email = "aldawaa@pharmalink.com",
                Address = "King Fahd Road, Building 10",
                City = "Riyadh",
                IsOpen = true,
                IsActive = true,
                OwnerId = pharmacist?.Id
            },
            new()
            {
                Name = "Al-Nahdi Pharmacy",
                Description = "Trusted pharmacy with over 20 years of experience serving the community with quality medicines and professional advice.",
                Phone = "+966509876543",
                Email = "alnahdi@pharmalink.com",
                Address = "Prince Sultan Road, Plaza 3",
                City = "Jeddah",
                IsOpen = true,
                IsActive = true
            },
            new()
            {
                Name = "Care Pharmacy",
                Description = "Your health partner offering 24/7 service with home delivery options and specialized consultation.",
                Phone = "+966551112233",
                Email = "care@pharmalink.com",
                Address = "Olaya Street, Tower 7",
                City = "Riyadh",
                IsOpen = false,
                IsActive = true
            },
            new()
            {
                Name = "MedPlus Pharmacy",
                Description = "Modern pharmacy with digital prescription management and fast service.",
                Phone = "+966554443322",
                Email = "medplus@pharmalink.com",
                Address = "Tahlia Street, Block B",
                City = "Jeddah",
                IsOpen = true,
                IsActive = true
            }
        };

        _context.Pharmacies.AddRange(pharmacies);
        await _context.SaveChangesAsync();
    }

    private async Task SeedMedicinesAsync()
    {
        if (await _context.Medicines.AnyAsync()) return;

        var medicines = new List<Medicine>
        {
            new() { Name = "Panadol 500mg", Description = "Effective pain relief and fever reduction tablets. Suitable for headaches, muscle pain, and cold symptoms.", Category = "Pain Relief", Price = 12.50m, Quantity = 500, ExpiryDate = DateTime.UtcNow.AddYears(2), RequiresPrescription = false },
            new() { Name = "Amoxicillin 500mg", Description = "Broad-spectrum antibiotic used to treat bacterial infections including respiratory, urinary, and skin infections.", Category = "Antibiotics", Price = 25.00m, Quantity = 200, RequiresPrescription = true, ExpiryDate = DateTime.UtcNow.AddYears(1) },
            new() { Name = "Vitamin D3 1000IU", Description = "Essential vitamin D supplement for bone health, immune support, and calcium absorption.", Category = "Vitamins", Price = 35.00m, Quantity = 300, ExpiryDate = DateTime.UtcNow.AddYears(3) },
            new() { Name = "Metformin 850mg", Description = "First-line medication for type 2 diabetes management. Helps control blood sugar levels.", Category = "Diabetes", Price = 18.00m, Quantity = 150, RequiresPrescription = true, ExpiryDate = DateTime.UtcNow.AddMonths(18) },
            new() { Name = "Omeprazole 20mg", Description = "Proton pump inhibitor for treating acid reflux, GERD, and stomach ulcers.", Category = "Digestive", Price = 22.00m, Quantity = 250, ExpiryDate = DateTime.UtcNow.AddYears(2) },
            new() { Name = "Cetirizine 10mg", Description = "Non-drowsy antihistamine for allergy relief including hay fever, hives, and allergic rhinitis.", Category = "Allergy", Price = 15.00m, Quantity = 400, ExpiryDate = DateTime.UtcNow.AddYears(2) },
            new() { Name = "Aspirin 100mg", Description = "Low-dose aspirin for cardiovascular protection and blood thinning.", Category = "Cardiovascular", Price = 8.50m, Quantity = 600, ExpiryDate = DateTime.UtcNow.AddYears(3) },
            new() { Name = "Ibuprofen 400mg", Description = "Non-steroidal anti-inflammatory drug for pain, inflammation, and fever.", Category = "Pain Relief", Price = 14.00m, Quantity = 350, ExpiryDate = DateTime.UtcNow.AddYears(2) },
            new() { Name = "Ventolin Inhaler", Description = "Quick-relief bronchodilator inhaler for asthma and COPD symptom management.", Category = "Respiratory", Price = 45.00m, Quantity = 80, RequiresPrescription = true, ExpiryDate = DateTime.UtcNow.AddYears(1) },
            new() { Name = "Vitamin C 1000mg", Description = "High-potency vitamin C for immune system support and antioxidant protection.", Category = "Vitamins", Price = 20.00m, Quantity = 500, ExpiryDate = DateTime.UtcNow.AddYears(3) },
            new() { Name = "Losartan 50mg", Description = "Angiotensin receptor blocker for high blood pressure and kidney protection.", Category = "Cardiovascular", Price = 28.00m, Quantity = 180, RequiresPrescription = true, ExpiryDate = DateTime.UtcNow.AddMonths(20) },
            new() { Name = "Azithromycin 250mg", Description = "Macrolide antibiotic for respiratory tract infections, skin infections, and STIs.", Category = "Antibiotics", Price = 32.00m, Quantity = 120, RequiresPrescription = true, ExpiryDate = DateTime.UtcNow.AddMonths(15) }
        };

        _context.Medicines.AddRange(medicines);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSuppliersAsync()
    {
        if (await _context.Suppliers.AnyAsync()) return;

        var suppliers = new List<Supplier>
        {
            new() { Name = "Pfizer Saudi Arabia", ContactPerson = "Ali Hassan Al-Qahtani", Phone = "+966501111111", Email = "ali.hassan@pfizer-sa.com", Address = "Industrial Area, Plot 45, Riyadh" },
            new() { Name = "Novartis Gulf", ContactPerson = "Sara Ahmed Al-Dosari", Phone = "+966502222222", Email = "sara.ahmed@novartis-gulf.com", Address = "Business District, Tower 12, Jeddah" },
            new() { Name = "SPIMACO", ContactPerson = "Khalid Omar Al-Shehri", Phone = "+966503333333", Email = "khalid.omar@spimaco.com", Address = "Pharmaceutical Zone, Building 8, Dammam" },
            new() { Name = "Tabuk Pharmaceuticals", ContactPerson = "Fatima Nasser", Phone = "+966504444444", Email = "fatima.n@tabukpharma.com", Address = "Tabuk Industrial City, Block C" }
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
            foreach (var medicine in medicines.Take(8))
            {
                _context.Inventories.Add(new Inventory
                {
                    PharmacyId = pharmacy.Id,
                    MedicineId = medicine.Id,
                    Quantity = random.Next(5, 120),
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
            var medicinesToAssign = medicines.OrderBy(_ => random.Next()).Take(6).ToList();
            foreach (var medicine in medicinesToAssign)
            {
                _context.SupplierMedicines.Add(new SupplierMedicine
                {
                    SupplierId = supplier.Id,
                    MedicineId = medicine.Id,
                    SupplyPrice = Math.Round(medicine.Price * 0.65m, 2),
                    AvailableQuantity = random.Next(50, 1000),
                    LastSupplyDate = DateTime.UtcNow.AddDays(-random.Next(1, 90))
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

        var prescriptionMedicines = await _context.Medicines.Where(m => m.RequiresPrescription).ToListAsync();

        // Prescription 1 - Approved
        var prescription1 = new Prescription
        {
            UserId = customer.Id,
            PrescriptionDate = DateTime.UtcNow.AddDays(-10),
            Status = PrescriptionStatus.Approved,
            DoctorName = "Dr. Ahmad Al-Zahrani",
            Notes = "Take medications after meals. Follow up in 2 weeks."
        };
        _context.Prescriptions.Add(prescription1);
        await _context.SaveChangesAsync();

        foreach (var med in prescriptionMedicines.Take(2))
        {
            _context.PrescriptionItems.Add(new PrescriptionItem
            {
                PrescriptionId = prescription1.Id,
                MedicineId = med.Id,
                Quantity = 2,
                DosageInstructions = "1 tablet twice daily after meals"
            });
        }

        // Prescription 2 - Pending
        var prescription2 = new Prescription
        {
            UserId = customer.Id,
            PrescriptionDate = DateTime.UtcNow.AddDays(-2),
            Status = PrescriptionStatus.Pending,
            DoctorName = "Dr. Fatima Al-Harbi",
            Notes = "Patient reports persistent cough. Review in 1 week."
        };
        _context.Prescriptions.Add(prescription2);
        await _context.SaveChangesAsync();

        if (prescriptionMedicines.Count > 2)
        {
            _context.PrescriptionItems.Add(new PrescriptionItem
            {
                PrescriptionId = prescription2.Id,
                MedicineId = prescriptionMedicines[2].Id,
                Quantity = 1,
                DosageInstructions = "Use inhaler as needed, max 4 times daily"
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedSalesAsync()
    {
        if (await _context.Sales.AnyAsync()) return;

        var customer = await _userManager.FindByEmailAsync("customer@pharmalink.com");
        var pharmacies = await _context.Pharmacies.Take(2).ToListAsync();
        if (customer == null || !pharmacies.Any()) return;

        var medicines = await _context.Medicines.Where(m => !m.RequiresPrescription).Take(5).ToListAsync();

        // Sale 1 - Completed
        var sale1 = new Sale
        {
            UserId = customer.Id,
            PharmacyId = pharmacies[0].Id,
            SaleDate = DateTime.UtcNow.AddDays(-5),
            TotalAmount = 0,
            Status = SaleStatus.Completed,
            Notes = "Regular purchase"
        };
        _context.Sales.Add(sale1);
        await _context.SaveChangesAsync();

        decimal total1 = 0;
        foreach (var med in medicines.Take(3))
        {
            var qty = 2;
            _context.SaleItems.Add(new SaleItem
            {
                SaleId = sale1.Id,
                MedicineId = med.Id,
                Quantity = qty,
                UnitPrice = med.Price
            });
            total1 += med.Price * qty;
        }
        sale1.TotalAmount = total1;

        // Sale 2 - Pending
        var sale2 = new Sale
        {
            UserId = customer.Id,
            PharmacyId = pharmacies.Count > 1 ? pharmacies[1].Id : pharmacies[0].Id,
            SaleDate = DateTime.UtcNow.AddDays(-1),
            TotalAmount = 0,
            Status = SaleStatus.Pending,
            Notes = "Awaiting payment confirmation"
        };
        _context.Sales.Add(sale2);
        await _context.SaveChangesAsync();

        decimal total2 = 0;
        foreach (var med in medicines.Skip(2).Take(2))
        {
            var qty = 1;
            _context.SaleItems.Add(new SaleItem
            {
                SaleId = sale2.Id,
                MedicineId = med.Id,
                Quantity = qty,
                UnitPrice = med.Price
            });
            total2 += med.Price * qty;
        }
        sale2.TotalAmount = total2;

        await _context.SaveChangesAsync();
    }
}