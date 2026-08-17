using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Controllers;

[Authorize]
public class ReviewController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int pharmacyId, int rating, string? comment)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        if (rating < 1 || rating > 5)
        {
            TempData["Error"] = "Rating must be between 1 and 5.";
            return RedirectToAction("Details", "Pharmacy", new { id = pharmacyId });
        }

        var review = new Review
        {
            UserId = userId,
            PharmacyId = pharmacyId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Review submitted successfully.";
        return RedirectToAction("Details", "Pharmacy", new { id = pharmacyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        var review = await _context.Reviews.FindAsync(id);

        if (review == null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        if (review.UserId != userId && !isAdmin)
            return Forbid();

        var pharmacyId = review.PharmacyId;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Review deleted successfully.";
        return RedirectToAction("Details", "Pharmacy", new { id = pharmacyId });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Title = "All Reviews";
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Pharmacy)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewViewModel
            {
                Id = r.Id,
                UserName = r.User.FirstName + " " + r.User.LastName,
                UserId = r.UserId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                PharmacyId = r.PharmacyId,
                PharmacyName = r.Pharmacy.Name
            })
            .ToListAsync();

        return View(reviews);
    }
}