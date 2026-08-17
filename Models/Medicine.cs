using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class Medicine
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(300)]
    [Display(Name = "Scientific Name")]
    public string ScientificName { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    [Display(Name = "Commercial Name")]
    public string CommercialName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    [StringLength(100)]
    [Display(Name = "Dosage Form")]
    public string? DosageForm { get; set; }

    [StringLength(50)]
    public string? Strength { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    [Display(Name = "Requires Prescription")]
    public bool RequiresPrescription { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CategoryId")]
    public MedicineCategory Category { get; set; } = null!;

    [ForeignKey("BrandId")]
    public MedicineBrand? Brand { get; set; }

    public ICollection<MedicineImage> Images { get; set; } = new List<MedicineImage>();
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
    public ICollection<FavoriteMedicine> FavoritedBy { get; set; } = new List<FavoriteMedicine>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}