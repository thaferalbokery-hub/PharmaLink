using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Admin,Pharmacist")]
public class SupplierController : Controller
{
    private readonly ApplicationDbContext _context;

    public SupplierController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Title = "Suppliers";
        ViewData["CurrentSearch"] = search;
        var query = _context.Suppliers.Include(s => s.SupplierMedicines).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.Contains(search) || s.ContactPerson.Contains(search));
        return View(await query.OrderBy(s => s.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.SupplierMedicines).ThenInclude(sm => sm.Medicine)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (supplier == null) return NotFound();
        ViewBag.Title = supplier.Name;
        return View(supplier);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (!ModelState.IsValid) return View(supplier);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Supplier created.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        return View(supplier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier supplier)
    {
        if (id != supplier.Id) return NotFound();
        if (!ModelState.IsValid) return View(supplier);
        _context.Update(supplier);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Supplier updated.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        return View(supplier);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Supplier deleted.";
        return RedirectToAction("Index");
    }
}