using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;

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
        ViewBag.Title = "PharmaLink - Home";
        ViewBag.TotalMedicines = await _context.Medicines.CountAsync(m => m.IsActive);
        ViewBag.TotalPharmacies = await _context.Pharmacies.CountAsync(p => p.IsActive);
        ViewBag.TotalSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive);
        ViewBag.TotalSales = await _context.Sales.CountAsync();
        return View();
    }

    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}