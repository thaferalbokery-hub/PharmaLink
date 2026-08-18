using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    // GET: Sale - with row-level filtering
    public async Task<IActionResult> Index(SaleStatus? status, DateTime? fromDate, DateTime? toDate, int? pharmacyId)
    {
        ViewBag.Title = "Sales";
        ViewData["CurrentStatus"] = status;
        ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
        ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
        ViewData["CurrentPharmacy"] = pharmacyId;
        ViewBag.Pharmacies = await _context.Pharmacies.Where(p => p.IsActive).ToListAsync();

        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Pharmacist");

        // Eager loading with Include/ThenInclude
        var query = _context.Sales
            .Include(s => s.User)
            .Include(s => s.Pharmacy)
            .Include(s => s.SaleItems).ThenInclude(si => si.Medicine)
            .AsQueryable();

        // Row-level filtering: customers see only their own sales
        if (!isAdmin)
            query = query.Where(s => s.UserId == user!.Id);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);
        if (fromDate.HasValue)
            query = query.Where(s => s.SaleDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(s => s.SaleDate <= toDate.Value);
        if (pharmacyId.HasValue)
            query = query.Where(s => s.PharmacyId == pharmacyId.Value);

        var sales = await query.OrderByDescending(s => s.SaleDate).ToListAsync();
        return View(sales);
    }

    // GET: Sale/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.User)
            .Include(s => s.Pharmacy)
            .Include(s => s.SaleItems).ThenInclude(si => si.Medicine)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null) return NotFound();

        // Authorization: customers can only view their own sales
        var user = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Admin") && !User.IsInRole("Pharmacist") && sale.UserId != user!.Id)
            return Forbid();

        ViewBag.Title = $"Sale #{sale.Id}";
        return View(sale);
    }

    // GET: Sale/Create
    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "New Sale";
        await PopulateSaleDropdowns();
        return View();
    }

    // POST: Sale/Create
    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string userId, int pharmacyId, string? notes,
        int[] medicineIds, int[] quantities)
    {
        if (medicineIds == null || medicineIds.Length == 0)
        {
            TempData["Error"] = "Please add at least one medicine to the sale.";
            await PopulateSaleDropdowns();
            return View();
        }

        // Validate stock availability before processing
        for (int i = 0; i < medicineIds.Length; i++)
        {
            var qty = quantities.Length > i ? quantities[i] : 1;
            if (qty <= 0)
            {
                TempData["Error"] = "Quantity must be greater than zero.";
                await PopulateSaleDropdowns();
                return View();
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(inv => inv.PharmacyId == pharmacyId && inv.MedicineId == medicineIds[i]);

            if (inventory == null || inventory.Quantity < qty)
            {
                var medicine = await _context.Medicines.FindAsync(medicineIds[i]);
                TempData["Error"] = $"Insufficient stock for {medicine?.Name ?? "medicine"}. Available: {inventory?.Quantity ?? 0}, Requested: {qty}";
                await PopulateSaleDropdowns();
                return View();
            }
        }

        // Create sale - calculate totals server-side
        var sale = new Sale
        {
            UserId = userId,
            PharmacyId = pharmacyId,
            SaleDate = DateTime.UtcNow,
            Status = SaleStatus.Completed,
            Notes = notes,
            TotalAmount = 0
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        decimal totalAmount = 0;
        for (int i = 0; i < medicineIds.Length; i++)
        {
            var medicine = await _context.Medicines.FindAsync(medicineIds[i]);
            if (medicine == null) continue;

            var qty = quantities.Length > i ? quantities[i] : 1;
            var unitPrice = medicine.Price; // Server-side price, never trust browser

            _context.SaleItems.Add(new SaleItem
            {
                SaleId = sale.Id,
                MedicineId = medicineIds[i],
                Quantity = qty,
                UnitPrice = unitPrice
            });

            totalAmount += unitPrice * qty;

            // Update inventory - decrease stock
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(inv => inv.PharmacyId == pharmacyId && inv.MedicineId == medicineIds[i]);
            if (inventory != null)
            {
                inventory.Quantity -= qty;
                inventory.LastUpdated = DateTime.UtcNow;
            }
        }

        sale.TotalAmount = totalAmount;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Sale #{sale.Id} created successfully. Total: {totalAmount:C}";
        return RedirectToAction("Details", new { id = sale.Id });
    }

    // GET: Sale/Edit/5
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.SaleItems).ThenInclude(si => si.Medicine)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sale == null) return NotFound();

        ViewBag.Title = "Edit Sale";
        ViewBag.Statuses = Enum.GetValues<SaleStatus>()
            .Select(s => new SelectListItem { Value = ((int)s).ToString(), Text = s.ToString(), Selected = s == sale.Status });
        return View(sale);
    }

    // POST: Sale/Edit/5
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SaleStatus status, string? notes)
    {
        var sale = await _context.Sales.FindAsync(id);
        if (sale == null) return NotFound();

        sale.Status = status;
        sale.Notes = notes;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Sale updated successfully.";
        return RedirectToAction("Details", new { id });
    }

    // POST: Sale/Delete/5
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.SaleItems).ThenInclude(si => si.Medicine)
            .Include(s => s.Pharmacy)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sale == null) return NotFound();
        ViewBag.Title = "Delete Sale";
        return View(sale);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.SaleItems)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sale == null) return NotFound();

        // Restore inventory if sale was completed
        if (sale.Status == SaleStatus.Completed)
        {
            foreach (var item in sale.SaleItems)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.PharmacyId == sale.PharmacyId && i.MedicineId == item.MedicineId);
                if (inventory != null)
                {
                    inventory.Quantity += item.Quantity;
                    inventory.LastUpdated = DateTime.UtcNow;
                }
            }
        }

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Sale deleted and inventory restored.";
        return RedirectToAction("Index");
    }

    private async Task PopulateSaleDropdowns()
    {
        ViewBag.Pharmacies = new SelectList(
            await _context.Pharmacies.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
        ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
        ViewBag.Customers = await _userManager.GetUsersInRoleAsync("Customer");
    }
}