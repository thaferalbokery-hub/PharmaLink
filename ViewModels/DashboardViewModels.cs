using PharmaLink.Models;

namespace PharmaLink.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalPharmacists { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalPharmacies { get; set; }
    public int TotalMedicines { get; set; }
    public int AvailableMedicines { get; set; }
    public int LowStockMedicines { get; set; }
    public int OutOfStockMedicines { get; set; }
    public int OpenPharmacies { get; set; }
    public int ClosedPharmacies { get; set; }
    public int TotalReviews { get; set; }
    public int TotalCategories { get; set; }
    public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
}

public class PharmacistDashboardViewModel
{
    public int PharmacyId { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public int TotalMedicines { get; set; }
    public int AvailableMedicines { get; set; }
    public int LowStockMedicines { get; set; }
    public int OutOfStockMedicines { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<InventoryItemViewModel> RecentUpdates { get; set; } = new();
    public List<InventoryItemViewModel> LowStockItems { get; set; } = new();
}

public class RecentActivityViewModel
{
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class ReviewViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PharmacyId { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
}

public class ReportViewModel
{
    public int TotalPharmacies { get; set; }
    public int TotalMedicines { get; set; }
    public int TotalCustomers { get; set; }
    public int AvailableCount { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int OpenPharmacies { get; set; }
    public int ClosedPharmacies { get; set; }
    public List<TopSearchedMedicine> MostSearchedMedicines { get; set; } = new();
    public List<TopFavoritedPharmacy> MostFavoritedPharmacies { get; set; } = new();
}

public class TopSearchedMedicine
{
    public string SearchTerm { get; set; } = string.Empty;
    public int SearchCount { get; set; }
}

public class TopFavoritedPharmacy
{
    public string PharmacyName { get; set; } = string.Empty;
    public int FavoriteCount { get; set; }
}

public class PharmacistReportViewModel
{
    public string PharmacyName { get; set; } = string.Empty;
    public int TotalInventory { get; set; }
    public int AvailableCount { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int RecentUpdatesCount { get; set; }
    public decimal AveragePrice { get; set; }
    public List<InventoryItemViewModel> RecentPriceUpdates { get; set; } = new();
}