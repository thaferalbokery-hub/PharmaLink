using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Admin,Pharmacist")]
public class PharmacistController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PharmacistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Pharmacist Dashboard";
        var user = await _userManager.GetUserAsync(User);
        var pharmacy = await _context.Pharmacies.Include(p => p.Inventories).FirstOrDefaultAsync(p => p.OwnerId == user!.Id);

        if (pharmacy != null)
        {
            ViewBag.PharmacyName = pharmacy.Name;
            ViewBag.TotalInventory = pharmacy.Inventories.Count;
            ViewBag.LowStock = pharmacy.Inventories.Count(i => i.Quantity <= i.MinimumStockLevel);
            ViewBag.PharmacyId = pharmacy.Id;
        }

        ViewBag.PendingPrescriptions = await _context.Prescriptions.CountAsync(p => p.Status == PrescriptionStatus.Pending);
        ViewBag.RecentSales = await _context.Sales.CountAsync(s => s.SaleDate >= DateTime.UtcNow.AddDays(-7));
        return View();
    }
}