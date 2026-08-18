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
        return View();
    }

    public async Task<IActionResult> Users()
    {
        ViewBag.Title = "User Management";
        var users = await _context.Users.ToListAsync();
        return View(users);
    }
}