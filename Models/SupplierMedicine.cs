using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class SupplierMedicine
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Supplier")]
    public int SupplierId { get; set; }

    [Required]
    [Display(Name = "Medicine")]
    public int MedicineId { get; set; }

    [Required]
    [Range(0.01, 99999.99)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Supply Price")]
    [DataType(DataType.Currency)]
    public decimal SupplyPrice { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    [Display(Name = "Available Quantity")]
    public int AvailableQuantity { get; set; }

    [Display(Name = "Last Supply Date")]
    [DataType(DataType.Date)]
    public DateTime? LastSupplyDate { get; set; }

    // Navigation Properties (M:N join entity)
    [ForeignKey("SupplierId")]
    public Supplier Supplier { get; set; } = null!;

    [ForeignKey("MedicineId")]
    public Medicine Medicine { get; set; } = null!;
}