using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class Inventory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Pharmacy")]
    public int PharmacyId { get; set; }

    [Required]
    [Display(Name = "Medicine")]
    public int MedicineId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    [Display(Name = "Minimum Stock Level")]
    public int MinimumStockLevel { get; set; } = 10;

    [Display(Name = "Last Updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("PharmacyId")]
    public Pharmacy Pharmacy { get; set; } = null!;

    [ForeignKey("MedicineId")]
    public Medicine Medicine { get; set; } = null!;

    // Computed property
    [NotMapped]
    public bool IsLowStock => Quantity <= MinimumStockLevel;
}