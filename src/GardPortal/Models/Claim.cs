using System.ComponentModel.DataAnnotations;

namespace GardPortal.Models;

public class Claim
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string ClaimNumber { get; set; } = string.Empty;

    public int PolicyId { get; set; }

    public ClaimCategory Category { get; set; }

    public ClaimStatus Status { get; set; } = ClaimStatus.Reported;

    public DateTime IncidentDate { get; set; }

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public decimal EstimatedAmount { get; set; }

    public decimal? SettledAmount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Policy Policy { get; set; } = null!;
    public ICollection<ClaimHistory> History { get; set; } = new List<ClaimHistory>();
}
