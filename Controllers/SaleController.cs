using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Controllers;

[Authorize]
public class SaleController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SaleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(SaleStatus? status, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.Title = "Sales";
        ViewData["CurrentStatus"] = status;

        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Pharmacist");

        var query = _context.Sales
            .Include(s => s.User)
            .Include(s => s.Pharmacy)
            .Include(s => s.SaleItems).ThenInclude(si => si.Medicine)
            .AsQueryable();

        if (!isAdmin)
            query = query.Where(s => s.UserId == user!.Id);
        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);
        if (fromDate.HasValue)
            query = query.Where(s => s.SaleDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(s => s.SaleDate <= toDate.Value);

        return View(await query.OrderByDescending(s => s.SaleDate).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.User)
            .Include(s => s.Pharmacy)
            .Include(s => s.SaleItems).ThenInclude(si => si.Medicine)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sale == null) return NotFound();
        return View(sale);
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Pharmacies = await _context.Pharmacies.Where(p => p.IsActive).ToListAsync();
        ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
        ViewBag.Customers = await _userManager.GetUsersInRoleAsync("Customer");
        return View();
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Sale sale, int[] medicineIds, int[] quantities)
    {
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        decimal total = 0;
        for (int i = 0; i < medicineIds.Length; i++)
        {
            var medicine = await _context.Medicines.FindAsync(medicineIds[i]);
            if (medicine == null) continue;
            var qty = quantities.Length > i ? quantities[i] : 1;
            _context.SaleItems.Add(new SaleItem
            {
                SaleId = sale.Id,
                MedicineId = medicineIds[i],
                Quantity = qty,
                UnitPrice = medicine.Price
            });
            total += medicine.Price * qty;
        }
        sale.TotalAmount = total;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Sale created.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, SaleStatus status)
    {
        var sale = await _context.Sales.FindAsync(id);
        if (sale == null) return NotFound();
        sale.Status = status;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Sale status updated.";
        return RedirectToAction("Details", new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var sale = await _context.Sales.FindAsync(id);
        if (sale == null) return NotFound();
        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Sale deleted.";
        return RedirectToAction("Index");
    }
}