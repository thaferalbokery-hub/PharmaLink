using System.ComponentModel.DataAnnotations;
using PharmaLink.Models;

namespace PharmaLink.ViewModels;

public class PharmacyCreateViewModel
{
    [Required(ErrorMessage = "Pharmacy name is required")]
    [StringLength(200)]
    [Display(Name = "Pharmacy Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? OwnerId { get; set; }
}

public class PharmacyEditViewModel : PharmacyCreateViewModel
{
    public int Id { get; set; }
    public bool IsOpen { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? Image { get; set; }
}

public class PharmacyDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int AvailableMedicineCount { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<PharmacyWorkingHour> WorkingHours { get; set; } = new();
    public List<PharmacyContact> Contacts { get; set; } = new();
    public List<InventoryItemViewModel> Inventory { get; set; } = new();
    public List<ReviewViewModel> Reviews { get; set; } = new();
    public bool IsFavorited { get; set; }
}

public class PharmacyListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsOpen { get; set; }
    public string? ImageUrl { get; set; }
    public int AvailableMedicineCount { get; set; }
    public double AverageRating { get; set; }
}

public class PharmacySearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? City { get; set; }
    public bool? IsOpen { get; set; }
    public List<PharmacyListViewModel> Results { get; set; } = new();
    public List<string> Cities { get; set; } = new();
}