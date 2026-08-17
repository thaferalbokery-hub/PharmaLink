using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class SearchHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string SearchTerm { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SearchType { get; set; } // Medicine, Pharmacy

    public DateTime SearchDate { get; set; } = DateTime.UtcNow;

    // Navigation Property
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;
}