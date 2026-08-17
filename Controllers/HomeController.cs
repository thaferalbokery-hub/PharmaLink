using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "PharmaLink - Pharmacy & Medicine Availability";

        var openPharmacies = await _context.Pharmacies
            .Where(p => p.IsActive && p.IsOpen)
            .Take(6)
            .Select(p => new PharmacyListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                City = p.City,
                Phone = p.Phone,
                IsOpen = p.IsOpen,
                ImageUrl = p.ImageUrl,
                AvailableMedicineCount = p.Inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available),
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0
            })
            .ToListAsync();

        var categories = await _context.MedicineCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var recentMedicines = await _context.Medicines
            .Where(m => m.IsActive)
            .Include(m => m.Category)
            .Include(m => m.Images)
            .OrderByDescending(m => m.UpdatedAt)
            .Take(8)
            .Select(m => new MedicineListViewModel
            {
                Id = m.Id,
                ScientificName = m.ScientificName,
                CommercialName = m.CommercialName,
                CategoryName = m.Category.Name,
                DosageForm = m.DosageForm,
                Strength = m.Strength,
                PrimaryImageUrl = m.Images.FirstOrDefault(i => i.IsPrimary) != null
                    ? m.Images.First(i => i.IsPrimary).ImageUrl : null,
                PharmacyCount = m.Inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available)
            })
            .ToListAsync();

        ViewBag.OpenPharmacies = openPharmacies;
        ViewBag.Categories = categories;
        ViewBag.RecentMedicines = recentMedicines;
        ViewData["TotalPharmacies"] = await _context.Pharmacies.CountAsync(p => p.IsActive);
        ViewData["TotalMedicines"] = await _context.Medicines.CountAsync(m => m.IsActive);

        return View();
    }

    public IActionResult About()
    {
        ViewBag.Title = "About PharmaLink";
        return View();
    }

    public IActionResult Privacy()
    {
        ViewBag.Title = "Privacy Policy";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}