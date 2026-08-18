using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class PrescriptionItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PrescriptionId { get; set; }

    [Required]
    [Display(Name = "Medicine")]
    public int MedicineId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    [StringLength(500)]
    [Display(Name = "Dosage Instructions")]
    public string? DosageInstructions { get; set; }

    // Navigation Properties
    [ForeignKey("PrescriptionId")]
    public Prescription Prescription { get; set; } = null!;

    [ForeignKey("MedicineId")]
    public Medicine Medicine { get; set; } = null!;
}