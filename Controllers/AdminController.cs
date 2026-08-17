using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.Services;
using PharmaLink.ViewModels;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IReportService _reportService;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IReportService reportService)
    {
        _context = context;
        _userManager = userManager;
        _reportService = reportService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Admin Dashboard";

        var model = new AdminDashboardViewModel
        {
            TotalUsers = await _context.Users.CountAsync(),
            TotalPharmacists = (await _userManager.GetUsersInRoleAsync("Pharmacist")).Count,
            TotalCustomers = (await _userManager.GetUsersInRoleAsync("Customer")).Count,
            TotalPharmacies = await _context.Pharmacies.CountAsync(p => p.IsActive),
            TotalMedicines = await _context.Medicines.CountAsync(m => m.IsActive),
            AvailableMedicines = await _context.Inventories.CountAsync(i => i.AvailabilityStatus == AvailabilityStatus.Available),
            LowStockMedicines = await _context.Inventories.CountAsync(i => i.AvailabilityStatus == AvailabilityStatus.LowStock),
            OutOfStockMedicines = await _context.Inventories.CountAsync(i => i.AvailabilityStatus == AvailabilityStatus.OutOfStock),
            OpenPharmacies = await _context.Pharmacies.CountAsync(p => p.IsActive && p.IsOpen),
            ClosedPharmacies = await _context.Pharmacies.CountAsync(p => p.IsActive && !p.IsOpen),
            TotalReviews = await _context.Reviews.CountAsync(),
            TotalCategories = await _context.MedicineCategories.CountAsync(c => c.IsActive)
        };

        return View(model);
    }

    // Users Management
    public async Task<IActionResult> Users()
    {
        ViewBag.Title = "User Management";
        var users = await _context.Users.ToListAsync();
        var userList = new List<(ApplicationUser User, string Role)>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add((user, roles.FirstOrDefault() ?? "No Role"));
        }

        ViewBag.Users = userList;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
        }
        return RedirectToAction("Users");
    }

    // Reports
    public async Task<IActionResult> Reports()
    {
        ViewBag.Title = "Admin Reports";
        var report = await _reportService.GetAdminReportAsync();
        return View(report);
    }
}