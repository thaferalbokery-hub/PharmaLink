using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public enum TransactionType
{
    StockAdded = 0,
    StockRemoved = 1,
    Adjustment = 2
}

public class InventoryTransaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PharmacyId { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public TransactionType TransactionType { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey("PharmacyId")]
    public Pharmacy Pharmacy { get; set; } = null!;

    [ForeignKey("MedicineId")]
    public Medicine Medicine { get; set; } = null!;

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;
}