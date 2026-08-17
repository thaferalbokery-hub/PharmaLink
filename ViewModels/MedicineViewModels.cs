using System.ComponentModel.DataAnnotations;
using PharmaLink.Models;

namespace PharmaLink.ViewModels;

public class MedicineCreateViewModel
{
    [Required(ErrorMessage = "Scientific name is required")]
    [StringLength(300)]
    [Display(Name = "Scientific Name")]
    public string ScientificName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Commercial name is required")]
    [StringLength(300)]
    [Display(Name = "Commercial Name")]
    public string CommercialName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "Brand")]
    public int? BrandId { get; set; }

    [StringLength(100)]
    [Display(Name = "Dosage Form")]
    public string? DosageForm { get; set; }

    [StringLength(50)]
    public string? Strength { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    [Display(Name = "Requires Prescription")]
    public bool RequiresPrescription { get; set; }

    public IFormFile? Image { get; set; }

    public List<MedicineCategory> Categories { get; set; } = new();
    public List<MedicineBrand> Brands { get; set; } = new();
}

public class MedicineEditViewModel : MedicineCreateViewModel
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ExistingImageUrl { get; set; }
}

public class MedicineDetailsViewModel
{
    public int Id { get; set; }
    public string ScientificName { get; set; } = string.Empty;
    public string CommercialName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public string? Unit { get; set; }
    public bool RequiresPrescription { get; set; }
    public List<MedicineImage> Images { get; set; } = new();
    public List<PharmacyAvailabilityViewModel> PharmacyAvailabilities { get; set; } = new();
    public bool IsFavorited { get; set; }
}

public class PharmacyAvailabilityViewModel
{
    public int PharmacyId { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public string PharmacyCity { get; set; } = string.Empty;
    public bool PharmacyIsOpen { get; set; }
    public decimal Price { get; set; }
    public AvailabilityStatus AvailabilityStatus { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class MedicineSearchViewModel
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public AvailabilityStatus? Availability { get; set; }
    public List<MedicineListViewModel> Results { get; set; } = new();
    public List<MedicineCategory> Categories { get; set; } = new();
    public List<MedicineBrand> Brands { get; set; } = new();
}

public class MedicineListViewModel
{
    public int Id { get; set; }
    public string ScientificName { get; set; } = string.Empty;
    public string CommercialName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public bool RequiresPrescription { get; set; }
    public int PharmacyCount { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}