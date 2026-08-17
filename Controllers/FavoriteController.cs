using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Controllers;

[Authorize]
public class FavoriteController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoriteController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "My Favorites";
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var favoritePharmacies = await _context.FavoritePharmacies
            .Where(f => f.UserId == userId)
            .Include(f => f.Pharmacy)
            .Select(f => f.Pharmacy)
            .ToListAsync();

        var favoriteMedicines = await _context.FavoriteMedicines
            .Where(f => f.UserId == userId)
            .Include(f => f.Medicine).ThenInclude(m => m.Category)
            .Include(f => f.Medicine).ThenInclude(m => m.Images)
            .Select(f => f.Medicine)
            .ToListAsync();

        ViewBag.FavoritePharmacies = favoritePharmacies;
        ViewBag.FavoriteMedicines = favoriteMedicines;
        return View();
    }
}