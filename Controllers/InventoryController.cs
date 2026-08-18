using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Admin,Pharmacist")]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? pharmacyId, bool? lowStock)
    {
        ViewBag.Title = "Inventory";
        ViewBag.Pharmacies = await _context.Pharmacies.Where(p => p.IsActive).ToListAsync();
        ViewData["CurrentPharmacy"] = pharmacyId;
        ViewData["LowStock"] = lowStock;

        var query = _context.Inventories
            .Include(i => i.Pharmacy)
            .Include(i => i.Medicine)
            .AsQueryable();

        if (pharmacyId.HasValue)
            query = query.Where(i => i.PharmacyId == pharmacyId.Value);
        if (lowStock == true)
            query = query.Where(i => i.Quantity <= i.MinimumStockLevel);

        return View(await query.OrderBy(i => i.Medicine.Name).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Pharmacies = await _context.Pharmacies.Where(p => p.IsActive).ToListAsync();
        ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inventory inventory)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Pharmacies = await _context.Pharmacies.Where(p => p.IsActive).ToListAsync();
            ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
            return View(inventory);
        }

        var exists = await _context.Inventories.AnyAsync(i => i.PharmacyId == inventory.PharmacyId && i.MedicineId == inventory.MedicineId);
        if (exists)
        {
            TempData["Error"] = "This medicine already exists in this pharmacy's inventory.";
            ViewBag.Pharmacies = await _context.Pharmacies.Where(p => p.IsActive).ToListAsync();
            ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
            return View(inventory);
        }

        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Inventory item added.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.Inventories.Include(i => i.Medicine).Include(i => i.Pharmacy).FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Inventory inventory)
    {
        if (id != inventory.Id) return NotFound();
        if (!ModelState.IsValid) return View(inventory);

        inventory.LastUpdated = DateTime.UtcNow;
        _context.Update(inventory);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Inventory updated.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.Inventories.FindAsync(id);
        if (item == null) return NotFound();
        _context.Inventories.Remove(item);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Inventory item removed.";
        return RedirectToAction("Index");
    }
}