using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.Services;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Admin")]
public class BrandController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public BrandController(ApplicationDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Medicine Brands";
        var brands = await _context.MedicineBrands
            .Include(b => b.Medicines)
            .OrderBy(b => b.Name)
            .ToListAsync();
        return View(brands);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Title = "Add Brand";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicineBrand model, IFormFile? logoFile)
    {
        if (!ModelState.IsValid) return View(model);

        if (logoFile != null)
        {
            var logoUrl = await _imageService.UploadImageAsync(logoFile);
            model.Logo = logoUrl;
        }

        _context.MedicineBrands.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Brand created successfully.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.Title = "Edit Brand";
        var brand = await _context.MedicineBrands.FindAsync(id);
        if (brand == null) return NotFound();
        return View(brand);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MedicineBrand model, IFormFile? logoFile)
    {
        if (!ModelState.IsValid) return View(model);

        var brand = await _context.MedicineBrands.FindAsync(model.Id);
        if (brand == null) return NotFound();

        brand.Name = model.Name;
        brand.Description = model.Description;
        brand.IsActive = model.IsActive;

        if (logoFile != null)
        {
            _imageService.DeleteImage(brand.Logo);
            brand.Logo = await _imageService.UploadImageAsync(logoFile);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Brand updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _context.MedicineBrands.FindAsync(id);
        if (brand != null)
        {
            brand.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Brand deleted successfully.";
        }
        return RedirectToAction("Index");
    }
}