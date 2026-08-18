using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class SaleItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SaleId { get; set; }

    [Required]
    [Display(Name = "Medicine")]
    public int MedicineId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Unit Price")]
    [DataType(DataType.Currency)]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Subtotal")]
    [DataType(DataType.Currency)]
    public decimal Subtotal => Quantity * UnitPrice;

    // Navigation Properties
    [ForeignKey("SaleId")]
    public Sale Sale { get; set; } = null!;

    [ForeignKey("MedicineId")]
    public Medicine Medicine { get; set; } = null!;
}