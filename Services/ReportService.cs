using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportViewModel> GetAdminReportAsync()
    {
        var report = new ReportViewModel
        {
            TotalPharmacies = await _context.Pharmacies.CountAsync(p => p.IsActive),
            TotalMedicines = await _context.Medicines.CountAsync(m => m.IsActive),
            TotalCustomers = await _context.Users.CountAsync(),
            AvailableCount = await _context.Inventories.CountAsync(i => i.AvailabilityStatus == AvailabilityStatus.Available),
            LowStockCount = await _context.Inventories.CountAsync(i => i.AvailabilityStatus == AvailabilityStatus.LowStock),
            OutOfStockCount = await _context.Inventories.CountAsync(i => i.AvailabilityStatus == AvailabilityStatus.OutOfStock),
            OpenPharmacies = await _context.Pharmacies.CountAsync(p => p.IsActive && p.IsOpen),
            ClosedPharmacies = await _context.Pharmacies.CountAsync(p => p.IsActive && !p.IsOpen),
            MostSearchedMedicines = await _context.SearchHistories
                .Where(s => s.SearchType == "Medicine")
                .GroupBy(s => s.SearchTerm)
                .Select(g => new TopSearchedMedicine
                {
                    SearchTerm = g.Key,
                    SearchCount = g.Count()
                })
                .OrderByDescending(x => x.SearchCount)
                .Take(10)
                .ToListAsync(),
            MostFavoritedPharmacies = await _context.FavoritePharmacies
                .Include(f => f.Pharmacy)
                .GroupBy(f => new { f.PharmacyId, f.Pharmacy.Name })
                .Select(g => new TopFavoritedPharmacy
                {
                    PharmacyName = g.Key.Name,
                    FavoriteCount = g.Count()
                })
                .OrderByDescending(x => x.FavoriteCount)
                .Take(10)
                .ToListAsync()
        };

        return report;
    }

    public async Task<PharmacistReportViewModel> GetPharmacistReportAsync(int pharmacyId)
    {
        var pharmacy = await _context.Pharmacies.FindAsync(pharmacyId);
        var inventories = await _context.Inventories
            .Where(i => i.PharmacyId == pharmacyId)
            .Include(i => i.Medicine).ThenInclude(m => m.Category)
            .ToListAsync();

        var report = new PharmacistReportViewModel
        {
            PharmacyName = pharmacy?.Name ?? "Unknown",
            TotalInventory = inventories.Count,
            AvailableCount = inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available),
            LowStockCount = inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.LowStock),
            OutOfStockCount = inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.OutOfStock),
            RecentUpdatesCount = inventories.Count(i => i.LastUpdated >= DateTime.UtcNow.AddDays(-7)),
            AveragePrice = inventories.Any() ? inventories.Average(i => i.Price) : 0,
            RecentPriceUpdates = inventories
                .OrderByDescending(i => i.LastUpdated)
                .Take(10)
                .Select(i => new InventoryItemViewModel
                {
                    Id = i.Id,
                    MedicineId = i.MedicineId,
                    MedicineName = i.Medicine.CommercialName,
                    MedicineScientificName = i.Medicine.ScientificName,
                    CategoryName = i.Medicine.Category.Name,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    AvailabilityStatus = i.AvailabilityStatus,
                    LastUpdated = i.LastUpdated
                }).ToList()
        };

        return report;
    }
}