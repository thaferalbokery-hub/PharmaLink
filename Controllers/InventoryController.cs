using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    // GET: Inventory - with row-level filtering
    public async Task<IActionResult> Index(int? pharmacyId, bool? lowStock, string? medicineName)
    {
        ViewBag.Title = "Inventory Management";
        ViewBag.Pharmacies = await _context.Pharmacies.Where(p => p.IsActive).ToListAsync();
        ViewData["CurrentPharmacy"] = pharmacyId;
        ViewData["LowStock"] = lowStock;
        ViewData["MedicineName"] = medicineName;

        // Eager loading with Include
        var query = _context.Inventories
            .Include(i => i.Pharmacy)
            .Include(i => i.Medicine)
            .AsQueryable();

        // Row-level filtering
        if (pharmacyId.HasValue)
            query = query.Where(i => i.PharmacyId == pharmacyId.Value);
        if (lowStock == true)
            query = query.Where(i => i.Quantity <= i.MinimumStockLevel);
        if (!string.IsNullOrWhiteSpace(medicineName))
            query = query.Where(i => i.Medicine.Name.Contains(medicineName));

        var inventories = await query.OrderBy(i => i.Pharmacy.Name).ThenBy(i => i.Medicine.Name).ToListAsync();
        return View(inventories);
    }

    // GET: Inventory/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "Add Inventory Item";
        await PopulateDropdowns();
        return View();
    }

    // POST: Inventory/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inventory inventory)
    {
        // Remove navigation property validation
        ModelState.Remove("Pharmacy");
        ModelState.Remove("Medicine");

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(inventory);
        }

        // Business logic: prevent duplicate pharmacy-medicine combination
        var exists = await _context.Inventories
            .AnyAsync(i => i.PharmacyId == inventory.PharmacyId && i.MedicineId == inventory.MedicineId);
        if (exists)
        {
            TempData["Error"] = "This medicine already exists in this pharmacy's inventory. Please edit the existing entry.";
            await PopulateDropdowns();
            return View(inventory);
        }

        // Business logic: quantity cannot be negative
        if (inventory.Quantity < 0)
        {
            TempData["Error"] = "Quantity cannot be negative.";
            await PopulateDropdowns();
            return View(inventory);
        }

        inventory.LastUpdated = DateTime.UtcNow;
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Inventory item added successfully.";
        return RedirectToAction("Index");
    }

    // GET: Inventory/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.Inventories
            .Include(i => i.Medicine)
            .Include(i => i.Pharmacy)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();
        ViewBag.Title = "Edit Inventory";
        return View(item);
    }

    // POST: Inventory/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, int quantity, int minimumStockLevel)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory == null) return NotFound();

        // Business logic: quantity cannot be negative
        if (quantity < 0)
        {
            TempData["Error"] = "Quantity cannot be negative.";
            var item = await _context.Inventories.Include(i => i.Medicine).Include(i => i.Pharmacy).FirstOrDefaultAsync(i => i.Id == id);
            return View(item);
        }

        inventory.Quantity = quantity;
        inventory.MinimumStockLevel = minimumStockLevel;
        inventory.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Inventory updated successfully.";
        return RedirectToAction("Index");
    }

    // POST: Inventory/Delete/5
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

    private async Task PopulateDropdowns()
    {
        ViewBag.PharmacyList = new SelectList(
            await _context.Pharmacies.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
        ViewBag.MedicineList = new SelectList(
            await _context.Medicines.Where(m => m.IsActive).ToListAsync(), "Id", "Name");
    }
}