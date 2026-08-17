using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.Services;
using PharmaLink.ViewModels;

namespace PharmaLink.Controllers;

[Authorize(Roles = "Pharmacist")]
public class PharmacistController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPharmacyService _pharmacyService;
    private readonly IInventoryService _inventoryService;
    private readonly IReportService _reportService;
    private readonly IImageService _imageService;

    public PharmacistController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPharmacyService pharmacyService,
        IInventoryService inventoryService,
        IReportService reportService,
        IImageService imageService)
    {
        _context = context;
        _userManager = userManager;
        _pharmacyService = pharmacyService;
        _inventoryService = inventoryService;
        _reportService = reportService;
        _imageService = imageService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "Pharmacist Dashboard";
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var pharmacy = await _pharmacyService.GetPharmacyByOwnerIdAsync(user.Id);
        if (pharmacy == null)
        {
            TempData["Warning"] = "You don't have a pharmacy assigned yet. Please contact admin.";
            return View(new PharmacistDashboardViewModel());
        }

        var inventory = await _inventoryService.GetPharmacyInventoryAsync(pharmacy.Id);
        var reviews = await _context.Reviews
            .Where(r => r.PharmacyId == pharmacy.Id)
            .ToListAsync();

        var model = new PharmacistDashboardViewModel
        {
            PharmacyId = pharmacy.Id,
            PharmacyName = pharmacy.Name,
            IsOpen = pharmacy.IsOpen,
            TotalMedicines = inventory.Count,
            AvailableMedicines = inventory.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available),
            LowStockMedicines = inventory.Count(i => i.AvailabilityStatus == AvailabilityStatus.LowStock),
            OutOfStockMedicines = inventory.Count(i => i.AvailabilityStatus == AvailabilityStatus.OutOfStock),
            AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
            ReviewCount = reviews.Count,
            RecentUpdates = inventory.OrderByDescending(i => i.LastUpdated).Take(5).ToList(),
            LowStockItems = inventory.Where(i => i.AvailabilityStatus == AvailabilityStatus.LowStock).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var pharmacy = await _pharmacyService.GetPharmacyByOwnerIdAsync(user.Id);
        if (pharmacy == null) return RedirectToAction("Index");

        await _pharmacyService.ToggleStatusAsync(pharmacy.Id);
        TempData["Success"] = $"Pharmacy status changed to {(pharmacy.IsOpen ? "Closed" : "Open")}.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Inventory()
    {
        ViewBag.Title = "My Inventory";
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var pharmacy = await _pharmacyService.GetPharmacyByOwnerIdAsync(user.Id);
        if (pharmacy == null) return RedirectToAction("Index");

        var inventory = await _inventoryService.GetPharmacyInventoryAsync(pharmacy.Id);
        ViewBag.PharmacyId = pharmacy.Id;
        ViewBag.PharmacyName = pharmacy.Name;
        return View(inventory);
    }

    [HttpGet]
    public async Task<IActionResult> AddInventory()
    {
        ViewBag.Title = "Add Medicine to Inventory";
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var pharmacy = await _pharmacyService.GetPharmacyByOwnerIdAsync(user.Id);
        if (pharmacy == null) return RedirectToAction("Index");

        var medicines = await _context.Medicines.Where(m => m.IsActive).OrderBy(m => m.CommercialName).ToListAsync();

        var model = new InventoryAddViewModel
        {
            PharmacyId = pharmacy.Id,
            AvailableMedicines = medicines
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddInventory(InventoryAddViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableMedicines = await _context.Medicines.Where(m => m.IsActive).OrderBy(m => m.CommercialName).ToListAsync();
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await _inventoryService.AddInventoryAsync(model, user.Id);
        TempData["Success"] = "Medicine added to inventory successfully.";
        return RedirectToAction("Inventory");
    }

    [HttpGet]
    public async Task<IActionResult> UpdateInventory(int id)
    {
        ViewBag.Title = "Update Inventory";
        var item = await _inventoryService.GetInventoryItemAsync(id);
        if (item == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var pharmacy = await _pharmacyService.GetPharmacyByOwnerIdAsync(user!.Id);
        if (pharmacy == null || item.PharmacyId != pharmacy.Id)
            return Forbid();

        var model = new InventoryUpdateViewModel
        {
            Id = item.Id,
            PharmacyId = item.PharmacyId,
            MedicineId = item.MedicineId,
            Quantity = item.Quantity,
            Price = item.Price,
            MedicineName = item.Medicine.CommercialName
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateInventory(InventoryUpdateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await _inventoryService.UpdateInventoryAsync(model, user.Id);
        TempData["Success"] = "Inventory updated successfully.";
        return RedirectToAction("Inventory");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveInventory(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var item = await _inventoryService.GetInventoryItemAsync(id);
        if (item == null) return NotFound();

        var pharmacy = await _pharmacyService.GetPharmacyByOwnerIdAsync(user.Id);
        if (pharmacy == null || item.PharmacyId != pharmacy.Id)
            return Forbid();

        await _inventoryService.RemoveInventoryAsync(id, user.Id);
        TempData["Success"] = "Item removed from inventory.";
        return RedirectToAction("Inventory");
    }

    public async Task<IActionResult> Reports()
    {
        ViewBag.Title = "Pharmacy Reports";
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var pharmacy = await _pharmacyService.GetPharmacyByOwnerIdAsync(user.Id);
        if (pharmacy == null) return RedirectToAction("Index");

        var report = await _reportService.GetPharmacistReportAsync(pharmacy.Id);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> EditPharmacy()
    {
        ViewBag.Title = "Edit Pharmacy";
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var pharmacy = await _pharmacyService.GetPharmacyByOwnerIdAsync(user.Id);
        if (pharmacy == null) return RedirectToAction("Index");

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

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPharmacy(PharmacyEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

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

    public async Task<IActionResult> WorkingHours()
    {
        ViewBag.Title = "Working Hours";
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var pharmacy = await _pharmacyService.GetPharmacyByOwnerIdAsync(user.Id);
        if (pharmacy == null) return RedirectToAction("Index");

        ViewBag.PharmacyName = pharmacy.Name;
        return View(pharmacy.WorkingHours.OrderBy(w => w.DayOfWeek).ToList());
    }
}