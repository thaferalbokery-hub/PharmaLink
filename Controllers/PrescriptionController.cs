using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    // GET: Prescription - with row-level filtering
    public async Task<IActionResult> Index(PrescriptionStatus? status, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.Title = "Prescriptions";
        ViewData["CurrentStatus"] = status;
        ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
        ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Pharmacist");

        // Eager loading
        var query = _context.Prescriptions
            .Include(p => p.User)
            .Include(p => p.PrescriptionItems).ThenInclude(pi => pi.Medicine)
            .AsQueryable();

        // Authorization: customers see only their own prescriptions
        if (!isAdmin)
            query = query.Where(p => p.UserId == user!.Id);

        // Row-level filtering
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);
        if (fromDate.HasValue)
            query = query.Where(p => p.PrescriptionDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.PrescriptionDate <= toDate.Value);

        var prescriptions = await query.OrderByDescending(p => p.PrescriptionDate).ToListAsync();
        return View(prescriptions);
    }

    // GET: Prescription/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.User)
            .Include(p => p.PrescriptionItems).ThenInclude(pi => pi.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prescription == null) return NotFound();

        // Authorization check
        var user = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Admin") && !User.IsInRole("Pharmacist") && prescription.UserId != user!.Id)
            return Forbid();

        ViewBag.Title = $"Prescription #{prescription.Id}";
        ViewBag.Statuses = Enum.GetValues<PrescriptionStatus>()
            .Select(s => new SelectListItem { Value = ((int)s).ToString(), Text = s.ToString() });
        return View(prescription);
    }

    // GET: Prescription/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Title = "New Prescription";
        ViewBag.Medicines = await _context.Medicines
            .Where(m => m.IsActive && m.RequiresPrescription)
            .OrderBy(m => m.Name)
            .ToListAsync();
        return View();
    }

    // POST: Prescription/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? doctorName, string? notes, DateTime? prescriptionDate,
        int[] medicineIds, int[] quantities, string[] dosages)
    {
        var user = await _userManager.GetUserAsync(User);

        if (medicineIds == null || medicineIds.Length == 0)
        {
            TempData["Error"] = "Please add at least one medicine to the prescription.";
            ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive && m.RequiresPrescription).ToListAsync();
            return View();
        }

        // Validate quantities
        for (int i = 0; i < medicineIds.Length; i++)
        {
            var qty = quantities.Length > i ? quantities[i] : 0;
            if (qty <= 0)
            {
                TempData["Error"] = "All quantities must be greater than zero.";
                ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive && m.RequiresPrescription).ToListAsync();
                return View();
            }

            // Validate medicine exists
            var medicineExists = await _context.Medicines.AnyAsync(m => m.Id == medicineIds[i]);
            if (!medicineExists)
            {
                TempData["Error"] = "Invalid medicine selected.";
                ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive && m.RequiresPrescription).ToListAsync();
                return View();
            }
        }

        var prescription = new Prescription
        {
            UserId = user!.Id,
            PrescriptionDate = prescriptionDate ?? DateTime.UtcNow,
            Status = PrescriptionStatus.Pending,
            DoctorName = doctorName,
            Notes = notes
        };

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

        TempData["Success"] = "Prescription created successfully.";
        return RedirectToAction("Details", new { id = prescription.Id });
    }

    // GET: Prescription/Edit/5
    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.PrescriptionItems).ThenInclude(pi => pi.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prescription == null) return NotFound();

        ViewBag.Title = "Edit Prescription";
        ViewBag.Statuses = Enum.GetValues<PrescriptionStatus>()
            .Select(s => new SelectListItem { Value = ((int)s).ToString(), Text = s.ToString(), Selected = s == prescription.Status });
        ViewBag.Medicines = await _context.Medicines.Where(m => m.IsActive && m.RequiresPrescription).ToListAsync();
        return View(prescription);
    }

    // POST: Prescription/Edit/5
    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PrescriptionStatus status, string? doctorName, string? notes)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();

        prescription.Status = status;
        prescription.DoctorName = doctorName;
        prescription.Notes = notes;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Prescription updated successfully.";
        return RedirectToAction("Details", new { id });
    }

    // POST: Prescription/UpdateStatus
    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, PrescriptionStatus status)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();

        prescription.Status = status;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Prescription status updated to {status}.";
        return RedirectToAction("Details", new { id });
    }

    // GET: Prescription/Delete/5
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.User)
            .Include(p => p.PrescriptionItems).ThenInclude(pi => pi.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prescription == null) return NotFound();
        ViewBag.Title = "Delete Prescription";
        return View(prescription);
    }

    // POST: Prescription/Delete/5
    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.PrescriptionItems)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prescription == null) return NotFound();

        _context.Prescriptions.Remove(prescription);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Prescription deleted successfully.";
        return RedirectToAction("Index");
    }

    // POST: Prescription/AddItem
    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int prescriptionId, int medicineId, int quantity, string? dosageInstructions)
    {
        if (quantity <= 0)
        {
            TempData["Error"] = "Quantity must be greater than zero.";
            return RedirectToAction("Details", new { id = prescriptionId });
        }

        var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
        if (prescription == null) return NotFound();

        var medicine = await _context.Medicines.FindAsync(medicineId);
        if (medicine == null)
        {
            TempData["Error"] = "Invalid medicine.";
            return RedirectToAction("Details", new { id = prescriptionId });
        }

        _context.PrescriptionItems.Add(new PrescriptionItem
        {
            PrescriptionId = prescriptionId,
            MedicineId = medicineId,
            Quantity = quantity,
            DosageInstructions = dosageInstructions
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = $"{medicine.Name} added to prescription.";
        return RedirectToAction("Details", new { id = prescriptionId });
    }

    // POST: Prescription/RemoveItem
    [Authorize(Roles = "Admin,Pharmacist")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        var item = await _context.PrescriptionItems.FindAsync(itemId);
        if (item == null) return NotFound();

        var prescriptionId = item.PrescriptionId;
        _context.PrescriptionItems.Remove(item);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Item removed from prescription.";
        return RedirectToAction("Details", new { id = prescriptionId });
    }
}