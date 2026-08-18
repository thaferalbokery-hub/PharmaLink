using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

namespace PharmaLink.Controllers;

[Authorize]
public class PrescriptionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PrescriptionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(PrescriptionStatus? status)
    {
        ViewBag.Title = "Prescriptions";
        ViewData["CurrentStatus"] = status;

        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Pharmacist");

        var query = _context.Prescriptions
            .Include(p => p.User)
            .Include(p => p.PrescriptionItems).ThenInclude(pi => pi.Medicine)
            .AsQueryable();

        if (!isAdmin)
            query = query.Where(p => p.UserId == user!.Id);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        return View(await query.OrderByDescending(p => p.PrescriptionDate).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.User)
            .Include(p => p.PrescriptionItems).ThenInclude(pi => pi.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prescription == null) return NotFound();
        return View(prescription);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive && m.RequiresPrescription).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Prescription prescription, int[] medicineIds, int[] quantities, string[] dosages)
    {
        var user = await _userManager.GetUserAsync(User);
        prescription.UserId = user!.Id;

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        for (int i = 0; i < medicineIds.Length; i++)
        {
            _context.PrescriptionItems.Add(new PrescriptionItem
            {
                PrescriptionId = prescription.Id,
                MedicineId = medicineIds[i],
                Quantity = quantities.Length > i ? quantities[i] : 1,
                DosageInstructions = dosages.Length > i ? dosages[i] : null
            });
        }
        await _context.SaveChangesAsync();
        TempData["Success"] = "Prescription created.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, PrescriptionStatus status)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();
        prescription.Status = status;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Prescription status updated.";
        return RedirectToAction("Details", new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();
        _context.Prescriptions.Remove(prescription);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Prescription deleted.";
        return RedirectToAction("Index");
    }
}