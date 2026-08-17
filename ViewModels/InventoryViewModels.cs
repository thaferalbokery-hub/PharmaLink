using System.ComponentModel.DataAnnotations;
using PharmaLink.Models;

namespace PharmaLink.ViewModels;

public class InventoryItemViewModel
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string MedicineScientificName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public AvailabilityStatus AvailabilityStatus { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class InventoryUpdateViewModel
{
    public int Id { get; set; }

    [Required]
    public int PharmacyId { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    public string MedicineName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class InventoryAddViewModel
{
    [Required]
    public int PharmacyId { get; set; }

    [Required(ErrorMessage = "Medicine is required")]
    [Display(Name = "Medicine")]
    public int MedicineId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    public List<Medicine>? AvailableMedicines { get; set; }
}