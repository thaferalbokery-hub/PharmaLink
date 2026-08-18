using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Admin Dashboard";
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.TotalPharmacies = await _context.Pharmacies.CountAsync();
        ViewBag.TotalMedicines = await _context.Medicines.CountAsync();
        ViewBag.TotalSuppliers = await _context.Suppliers.CountAsync();
        ViewBag.TotalSales = await _context.Sales.CountAsync();
        ViewBag.TotalRevenue = await _context.Sales.Where(s => s.Status == SaleStatus.Completed).SumAsync(s => s.TotalAmount);
        ViewBag.LowStockCount = await _context.Inventories.CountAsync(i => i.Quantity <= i.MinimumStockLevel);
        ViewBag.PendingPrescriptions = await _context.Prescriptions.CountAsync(p => p.Status == PrescriptionStatus.Pending);

        // Prepare data for _DashboardCards partial view
        ViewBag.DashboardCards = new List<dynamic>
        {
            new { Icon = "fas fa-users", Color = "primary", Value = ViewBag.TotalUsers, Label = "Users" },
            new { Icon = "fas fa-clinic-medical", Color = "success", Value = ViewBag.TotalPharmacies, Label = "Pharmacies" },
            new { Icon = "fas fa-pills", Color = "info", Value = ViewBag.TotalMedicines, Label = "Medicines" },
            new { Icon = "fas fa-truck", Color = "warning", Value = ViewBag.TotalSuppliers, Label = "Suppliers" },
            new { Icon = "fas fa-cash-register", Color = "primary", Value = ViewBag.TotalSales, Label = "Total Sales" },
            new { Icon = "fas fa-dollar-sign", Color = "success", Value = ((decimal)ViewBag.TotalRevenue).ToString("C"), Label = "Revenue" },
            new { Icon = "fas fa-exclamation-triangle", Color = "danger", Value = ViewBag.LowStockCount, Label = "Low Stock" },
            new { Icon = "fas fa-clock", Color = "warning", Value = ViewBag.PendingPrescriptions, Label = "Pending Rx" }
        };

        return View();
    }

    public async Task<IActionResult> Users()
    {
        ViewBag.Title = "User Management";
        var users = await _context.Users.ToListAsync();
        return View(users);
    }
}