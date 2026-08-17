using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Medicine Categories";
        var categories = await _context.MedicineCategories
            .Include(c => c.Medicines)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Title = "Add Category";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicineCategory model)
    {
        if (!ModelState.IsValid) return View(model);

        _context.MedicineCategories.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category created successfully.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.Title = "Edit Category";
        var category = await _context.MedicineCategories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MedicineCategory model)
    {
        if (!ModelState.IsValid) return View(model);

        var category = await _context.MedicineCategories.FindAsync(model.Id);
        if (category == null) return NotFound();

        category.Name = model.Name;
        category.Description = model.Description;
        category.IsActive = model.IsActive;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Category updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.MedicineCategories.FindAsync(id);
        if (category != null)
        {
            category.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category deleted successfully.";
        }
        return RedirectToAction("Index");
    }
}