using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

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
        ViewBag.Title = "Reports";
        ViewBag.TotalMedicines = await _context.Medicines.CountAsync();
        ViewBag.TotalPharmacies = await _context.Pharmacies.CountAsync();
        ViewBag.TotalSuppliers = await _context.Suppliers.CountAsync();
        ViewBag.TotalSales = await _context.Sales.CountAsync();
        ViewBag.TotalRevenue = await _context.Sales.Where(s => s.Status == SaleStatus.Completed).SumAsync(s => s.TotalAmount);
        ViewBag.AverageSale = await _context.Sales.Where(s => s.Status == SaleStatus.Completed).AverageAsync(s => (decimal?)s.TotalAmount) ?? 0;
        ViewBag.LowStockItems = await _context.Inventories.CountAsync(i => i.Quantity <= i.MinimumStockLevel);
        ViewBag.TotalPrescriptions = await _context.Prescriptions.CountAsync();
        ViewBag.PendingPrescriptions = await _context.Prescriptions.CountAsync(p => p.Status == PrescriptionStatus.Pending);

        // Column projection example - only select needed fields
        ViewBag.TopMedicines = await _context.SaleItems
            .GroupBy(si => si.Medicine.Name)
            .Select(g => new { Name = g.Key, TotalSold = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.TotalSold)
            .Take(5)
            .ToListAsync();

        ViewBag.SalesByMonth = await _context.Sales
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= DateTime.UtcNow.AddMonths(-6))
            .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
            .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:D2}", Total = g.Sum(s => s.TotalAmount) })
            .OrderBy(x => x.Month)
            .ToListAsync();

        return View();
    }
}