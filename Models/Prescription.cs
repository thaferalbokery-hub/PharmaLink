using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public enum PrescriptionStatus
{
    Pending = 0,
    Approved = 1,
    Dispensed = 2,
    Rejected = 3,
    Cancelled = 4
}

public class Prescription
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Prescription Date")]
    public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;

    [Required]
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(200)]
    [Display(Name = "Doctor Name")]
    public string? DoctorName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}