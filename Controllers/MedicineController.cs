using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.Services;
using PharmaLink.ViewModels;

namespace PharmaLink.Controllers;

public class MedicineController : Controller
{
    private readonly IMedicineService _medicineService;
    private readonly IImageService _imageService;
    private readonly ISearchHistoryService _searchHistoryService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public MedicineController(
        IMedicineService medicineService,
        IImageService imageService,
        ISearchHistoryService searchHistoryService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _medicineService = medicineService;
        _imageService = imageService;
        _searchHistoryService = searchHistoryService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Medicines";
        var medicines = await _medicineService.GetAllMedicinesAsync();
        return View(medicines);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        var medicine = await _medicineService.GetMedicineDetailsAsync(id, userId);
        if (medicine == null) return NotFound();

        ViewBag.Title = medicine.CommercialName;
        return View(medicine);
    }

    public async Task<IActionResult> Search(string? q, int? categoryId, int? brandId, AvailabilityStatus? availability)
    {
        ViewBag.Title = "Search Medicines";

        if (!string.IsNullOrWhiteSpace(q) && User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
                await _searchHistoryService.RecordSearchAsync(userId, q, "Medicine");
        }

        var result = await _medicineService.SearchMedicinesAsync(q, categoryId, brandId, availability);
        ViewData["SearchResultCount"] = result.Results.Count;
        return View(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "Add Medicine";
        var model = new MedicineCreateViewModel
        {
            Categories = await _medicineService.GetAllCategoriesAsync(),
            Brands = await _medicineService.GetAllBrandsAsync()
        };
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicineCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _medicineService.GetAllCategoriesAsync();
            model.Brands = await _medicineService.GetAllBrandsAsync();
            return View(model);
        }

        var medicine = await _medicineService.CreateMedicineAsync(model);

        if (model.Image != null)
        {
            var imageUrl = await _imageService.UploadImageAsync(model.Image);
            if (imageUrl != null)
            {
                var medicineImage = new MedicineImage
                {
                    MedicineId = medicine.Id,
                    ImageUrl = imageUrl,
                    IsPrimary = true
                };
                _context.MedicineImages.Add(medicineImage);
                await _context.SaveChangesAsync();
            }
        }

        TempData["Success"] = "Medicine created successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.Title = "Edit Medicine";
        var medicine = await _medicineService.GetMedicineByIdAsync(id);
        if (medicine == null) return NotFound();

        var model = new MedicineEditViewModel
        {
            Id = medicine.Id,
            ScientificName = medicine.ScientificName,
            CommercialName = medicine.CommercialName,
            Description = medicine.Description,
            CategoryId = medicine.CategoryId,
            BrandId = medicine.BrandId,
            DosageForm = medicine.DosageForm,
            Strength = medicine.Strength,
            Unit = medicine.Unit,
            RequiresPrescription = medicine.RequiresPrescription,
            IsActive = medicine.IsActive,
            ExistingImageUrl = medicine.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ?? medicine.Images.FirstOrDefault()?.ImageUrl,
            Categories = await _medicineService.GetAllCategoriesAsync(),
            Brands = await _medicineService.GetAllBrandsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MedicineEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _medicineService.GetAllCategoriesAsync();
            model.Brands = await _medicineService.GetAllBrandsAsync();
            return View(model);
        }

        await _medicineService.UpdateMedicineAsync(model);

        if (model.Image != null)
        {
            var imageUrl = await _imageService.UploadImageAsync(model.Image);
            if (imageUrl != null)
            {
                _imageService.DeleteImage(model.ExistingImageUrl);
                var existingImage = await _context.MedicineImages
                    .FirstOrDefaultAsync(i => i.MedicineId == model.Id && i.IsPrimary);

                if (existingImage != null)
                {
                    existingImage.ImageUrl = imageUrl;
                    existingImage.UploadedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.MedicineImages.Add(new MedicineImage
                    {
                        MedicineId = model.Id,
                        ImageUrl = imageUrl,
                        IsPrimary = true
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        TempData["Success"] = "Medicine updated successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _medicineService.DeleteMedicineAsync(id);
        TempData["Success"] = "Medicine deleted successfully.";
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var existing = await _context.FavoriteMedicines
            .FirstOrDefaultAsync(f => f.UserId == userId && f.MedicineId == id);

        if (existing != null)
        {
            _context.FavoriteMedicines.Remove(existing);
            TempData["Success"] = "Removed from favorites.";
        }
        else
        {
            _context.FavoriteMedicines.Add(new FavoriteMedicine
            {
                UserId = userId,
                MedicineId = id
            });
            TempData["Success"] = "Added to favorites.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }
}