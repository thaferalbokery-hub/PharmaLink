using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.Services;

namespace PharmaLink.Controllers;

public class PharmacyController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public PharmacyController(ApplicationDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<IActionResult> Index(string? search, string? city)
    {
        ViewBag.Title = "Pharmacies";
        ViewBag.Cities = await _context.Pharmacies.Select(p => p.City).Distinct().OrderBy(c => c).ToListAsync();
        ViewData["CurrentSearch"] = search;
        ViewData["CurrentCity"] = city;

        var query = _context.Pharmacies.Where(p => p.IsActive).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Address.Contains(search));
        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => p.City == city);

        return View(await query.OrderBy(p => p.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var pharmacy = await _context.Pharmacies
            .Include(p => p.Inventories).ThenInclude(i => i.Medicine)
            .Include(p => p.Sales)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pharmacy == null) return NotFound();
        ViewBag.Title = pharmacy.Name;
        return View(pharmacy);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pharmacy pharmacy, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(pharmacy);
        if (imageFile != null)
            pharmacy.ImageUrl = await _imageService.UploadImageAsync(imageFile);
        _context.Pharmacies.Add(pharmacy);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Pharmacy created successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var pharmacy = await _context.Pharmacies.FindAsync(id);
        if (pharmacy == null) return NotFound();
        return View(pharmacy);
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Pharmacy pharmacy, IFormFile? imageFile)
    {
        if (id != pharmacy.Id) return NotFound();
        if (!ModelState.IsValid) return View(pharmacy);

        var existing = await _context.Pharmacies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (existing == null) return NotFound();

        if (imageFile != null)
        {
            _imageService.DeleteImage(existing.ImageUrl);
            pharmacy.ImageUrl = await _imageService.UploadImageAsync(imageFile);
        }
        else pharmacy.ImageUrl = existing.ImageUrl;

        pharmacy.CreatedAt = existing.CreatedAt;
        pharmacy.OwnerId = existing.OwnerId;
        _context.Update(pharmacy);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Pharmacy updated successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var pharmacy = await _context.Pharmacies.FindAsync(id);
        if (pharmacy == null) return NotFound();
        return View(pharmacy);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var pharmacy = await _context.Pharmacies.FindAsync(id);
        if (pharmacy == null) return NotFound();
        _imageService.DeleteImage(pharmacy.ImageUrl);
        _context.Pharmacies.Remove(pharmacy);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Pharmacy deleted.";
        return RedirectToAction("Index");
    }
}