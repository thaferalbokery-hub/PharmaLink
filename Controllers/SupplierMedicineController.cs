using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Admin,Pharmacist")]
public class SupplierMedicineController : Controller
{
    private readonly ApplicationDbContext _context;

    public SupplierMedicineController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Supplier-Medicine Relationships";
        var items = await _context.SupplierMedicines
            .Include(sm => sm.Supplier)
            .Include(sm => sm.Medicine)
            .OrderBy(sm => sm.Supplier.Name)
            .ToListAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierMedicine sm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
            return View(sm);
        }

        var exists = await _context.SupplierMedicines.AnyAsync(x => x.SupplierId == sm.SupplierId && x.MedicineId == sm.MedicineId);
        if (exists)
        {
            TempData["Error"] = "This supplier-medicine relationship already exists.";
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
            return View(sm);
        }

        _context.SupplierMedicines.Add(sm);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Supplier-Medicine link created.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sm = await _context.SupplierMedicines.Include(x => x.Supplier).Include(x => x.Medicine).FirstOrDefaultAsync(x => x.Id == id);
        if (sm == null) return NotFound();
        ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
        return View(sm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplierMedicine sm)
    {
        if (id != sm.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();
            return View(sm);
        }
        _context.Update(sm);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Updated.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var sm = await _context.SupplierMedicines.FindAsync(id);
        if (sm == null) return NotFound();
        _context.SupplierMedicines.Remove(sm);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Deleted.";
        return RedirectToAction("Index");
    }
}