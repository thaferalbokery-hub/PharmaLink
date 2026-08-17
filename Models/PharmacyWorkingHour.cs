using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class PharmacyWorkingHour
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PharmacyId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan OpeningTime { get; set; }

    public TimeSpan ClosingTime { get; set; }

    public bool IsClosed { get; set; } = false;

    // Navigation Property
    [ForeignKey("PharmacyId")]
    public Pharmacy Pharmacy { get; set; } = null!;
}