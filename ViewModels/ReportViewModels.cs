namespace PharmaLink.ViewModels;

/// <summary>
/// DTO for column-level projection - Medicine sales report.
/// NOT a database entity - used only for reporting queries with .Select()
/// </summary>
public class MedicineReportDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// DTO for column-level projection - Pharmacy sales summary.
/// NOT a database entity.
/// </summary>
public class PharmacySalesDto
{
    public string PharmacyName { get; set; } = string.Empty;
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// DTO for column-level projection - Low stock items.
/// NOT a database entity.
/// </summary>
public class LowStockDto
{
    public string PharmacyName { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumLevel { get; set; }
}

/// <summary>
/// DTO for column-level projection - Monthly sales summary.
/// NOT a database entity.
/// </summary>
public class MonthlySalesDto
{
    public string Month { get; set; } = string.Empty;
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// DTO for column-level projection - Medicine listing (only needed columns).
/// NOT a database entity.
/// </summary>
public class MedicineListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool RequiresPrescription { get; set; }
    public string? ImageUrl { get; set; }
}