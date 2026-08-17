using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.Services;
using PharmaLink.ViewModels;

namespace PharmaLink.Controllers;

public class PharmacyController : Controller
{
    private readonly IPharmacyService _pharmacyService;
    private readonly ISearchHistoryService _searchHistoryService;
    private readonly IImageService _imageService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public PharmacyController(
        IPharmacyService pharmacyService,
        ISearchHistoryService searchHistoryService,
        IImageService imageService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _pharmacyService = pharmacyService;
        _searchHistoryService = searchHistoryService;
        _imageService = imageService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Pharmacies";
        var pharmacies = await _pharmacyService.GetAllPharmaciesAsync();
        return View(pharmacies);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        var pharmacy = await _pharmacyService.GetPharmacyDetailsAsync(id, userId);
        if (pharmacy == null) return NotFound();

        ViewBag.Title = pharmacy.Name;
        return View(pharmacy);
    }

    public async Task<IActionResult> Search(string? q, string? city, bool? isOpen)
    {
        ViewBag.Title = "Search Pharmacies";

        if (!string.IsNullOrWhiteSpace(q) && User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
                await _searchHistoryService.RecordSearchAsync(userId, q, "Pharmacy");
        }

        var result = await _pharmacyService.SearchPharmaciesAsync(q, city, isOpen);
        ViewData["SearchResultCount"] = result.Results.Count;
        return View(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "Add Pharmacy";
        var pharmacists = await _userManager.GetUsersInRoleAsync("Pharmacist");
        ViewBag.Pharmacists = pharmacists;
        return View(new PharmacyCreateViewModel());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PharmacyCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var pharmacists = await _userManager.GetUsersInRoleAsync("Pharmacist");
            ViewBag.Pharmacists = pharmacists;
            return View(model);
        }

        await _pharmacyService.CreatePharmacyAsync(model);
        TempData["Success"] = "Pharmacy created successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.Title = "Edit Pharmacy";
        var pharmacy = await _pharmacyService.GetPharmacyByIdAsync(id);
        if (pharmacy == null) return NotFound();

        var model = new PharmacyEditViewModel
        {
            Id = pharmacy.Id,
            Name = pharmacy.Name,
            Description = pharmacy.Description,
            Phone = pharmacy.Phone,
            Email = pharmacy.Email,
            Address = pharmacy.Address,
            City = pharmacy.City,
            Latitude = pharmacy.Latitude,
            Longitude = pharmacy.Longitude,
            IsOpen = pharmacy.IsOpen,
            IsActive = pharmacy.IsActive,
            ImageUrl = pharmacy.ImageUrl
        };

        var pharmacists = await _userManager.GetUsersInRoleAsync("Pharmacist");
        ViewBag.Pharmacists = pharmacists;
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PharmacyEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var pharmacists = await _userManager.GetUsersInRoleAsync("Pharmacist");
            ViewBag.Pharmacists = pharmacists;
            return View(model);
        }

        if (model.Image != null)
        {
            var imageUrl = await _imageService.UploadImageAsync(model.Image);
            if (imageUrl != null)
            {
                _imageService.DeleteImage(model.ImageUrl);
                model.ImageUrl = imageUrl;
            }
        }

        await _pharmacyService.UpdatePharmacyAsync(model);
        TempData["Success"] = "Pharmacy updated successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _pharmacyService.DeletePharmacyAsync(id);
        TempData["Success"] = "Pharmacy deleted successfully.";
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var existing = await _context.FavoritePharmacies
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PharmacyId == id);

        if (existing != null)
        {
            _context.FavoritePharmacies.Remove(existing);
            TempData["Success"] = "Removed from favorites.";
        }
        else
        {
            _context.FavoritePharmacies.Add(new FavoritePharmacy
            {
                UserId = userId,
                PharmacyId = id
            });
            TempData["Success"] = "Added to favorites.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }
}