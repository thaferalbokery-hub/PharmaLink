using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Admin,Pharmacist")]
public class ReportController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Reports & Analytics";

        // Basic statistics
        ViewBag.TotalMedicines = await _context.Medicines.CountAsync();
        ViewBag.TotalPharmacies = await _context.Pharmacies.CountAsync();
        ViewBag.TotalSuppliers = await _context.Suppliers.CountAsync();
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.TotalSales = await _context.Sales.CountAsync();
        ViewBag.TotalPrescriptions = await _context.Prescriptions.CountAsync();

        // Revenue calculations using Sum() and Average()
        ViewBag.TotalRevenue = await _context.Sales
            .Where(s => s.Status == SaleStatus.Completed)
            .SumAsync(s => s.TotalAmount);

        ViewBag.AverageSale = await _context.Sales
            .Where(s => s.Status == SaleStatus.Completed)
            .AverageAsync(s => (decimal?)s.TotalAmount) ?? 0;

        // Inventory statistics
        ViewBag.LowStockItems = await _context.Inventories
            .CountAsync(i => i.Quantity <= i.MinimumStockLevel && i.Quantity > 0);

        ViewBag.OutOfStockItems = await _context.Inventories
            .CountAsync(i => i.Quantity == 0);

        // Prescription statistics
        ViewBag.PendingPrescriptions = await _context.Prescriptions
            .CountAsync(p => p.Status == PrescriptionStatus.Pending);

        ViewBag.ApprovedPrescriptions = await _context.Prescriptions
            .CountAsync(p => p.Status == PrescriptionStatus.Approved);

        // ============================================================
        // COLUMN-LEVEL PROJECTION using .Select()
        // Only retrieving specific columns needed for the report
        // ============================================================
        ViewBag.TopSellingMedicines = await _context.SaleItems
            .GroupBy(si => new { si.MedicineId, si.Medicine.Name })
            .Select(g => new MedicineReportDto
            {
                MedicineId = g.Key.MedicineId,
                MedicineName = g.Key.Name,
                TotalQuantitySold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(5)
            .ToListAsync();

        // Column projection: Sales by pharmacy (only needed columns)
        ViewBag.SalesByPharmacy = await _context.Sales
            .Where(s => s.Status == SaleStatus.Completed)
            .GroupBy(s => new { s.PharmacyId, s.Pharmacy.Name })
            .Select(g => new PharmacySalesDto
            {
                PharmacyName = g.Key.Name,
                TotalSales = g.Count(),
                TotalRevenue = g.Sum(s => s.TotalAmount)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync();

        // Column projection: Low stock medicines (only Id, Name, Category, Quantity)
        ViewBag.LowStockMedicines = await _context.Inventories
            .Where(i => i.Quantity <= i.MinimumStockLevel)
            .Select(i => new LowStockDto
            {
                PharmacyName = i.Pharmacy.Name,
                MedicineName = i.Medicine.Name,
                CurrentStock = i.Quantity,
                MinimumLevel = i.MinimumStockLevel
            })
            .OrderBy(x => x.CurrentStock)
            .Take(10)
            .ToListAsync();

        // Sales by month using GroupBy
        ViewBag.MonthlySales = await _context.Sales
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= DateTime.UtcNow.AddMonths(-6))
            .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
            .Select(g => new MonthlySalesDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                TotalSales = g.Count(),
                TotalRevenue = g.Sum(s => s.TotalAmount)
            })
            .OrderBy(x => x.Month)
            .ToListAsync();

        return View();
    }
}