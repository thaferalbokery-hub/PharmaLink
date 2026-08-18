using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.Services;

namespace PharmaLink.Controllers;

public class MedicineController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public MedicineController(ApplicationDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<IActionResult> Index(string? search, string? category)
    {
        ViewBag.Title = "Medicines";
        ViewBag.Categories = await _context.Medicines.Select(m => m.Category).Distinct().OrderBy(c => c).ToListAsync();
        ViewData["CurrentSearch"] = search;
        ViewData["CurrentCategory"] = category;

        var query = _context.Medicines.Where(m => m.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search) || m.Description!.Contains(search));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category == category);

        var medicines = await query.OrderBy(m => m.Name).ToListAsync();
        return View(medicines);
    }

    public async Task<IActionResult> Details(int id)
    {
        var medicine = await _context.Medicines
            .Include(m => m.Inventories).ThenInclude(i => i.Pharmacy)
            .Include(m => m.SupplierMedicines).ThenInclude(sm => sm.Supplier)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medicine == null) return NotFound();
        ViewBag.Title = medicine.Name;
        return View(medicine);
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Title = "Add Medicine";
        return View();
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Medicine medicine, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(medicine);

        if (imageFile != null)
            medicine.ImageUrl = await _imageService.UploadImageAsync(imageFile);

        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Medicine created successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();
        ViewBag.Title = "Edit Medicine";
        return View(medicine);
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Medicine medicine, IFormFile? imageFile)
    {
        if (id != medicine.Id) return NotFound();
        if (!ModelState.IsValid) return View(medicine);

        var existing = await _context.Medicines.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (existing == null) return NotFound();

        if (imageFile != null)
        {
            _imageService.DeleteImage(existing.ImageUrl);
            medicine.ImageUrl = await _imageService.UploadImageAsync(imageFile);
        }
        else
        {
            medicine.ImageUrl = existing.ImageUrl;
        }

        medicine.CreatedAt = existing.CreatedAt;
        _context.Update(medicine);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Medicine updated successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();
        return View(medicine);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();

        _imageService.DeleteImage(medicine.ImageUrl);
        _context.Medicines.Remove(medicine);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Medicine deleted successfully.";
        return RedirectToAction("Index");
    }
}