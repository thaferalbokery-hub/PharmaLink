using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public enum AvailabilityStatus
{
    Available = 0,
    LowStock = 1,
    OutOfStock = 2
}

public class Inventory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PharmacyId { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; } = 0;

    [Required]
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.OutOfStock;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties - Many-to-Many (Medicine <-> Pharmacy through Inventory)
    [ForeignKey("PharmacyId")]
    public Pharmacy Pharmacy { get; set; } = null!;

    [ForeignKey("MedicineId")]
    public Medicine Medicine { get; set; } = null!;
}