using System.ComponentModel.DataAnnotations;

namespace GardPortal.Models;

public class ClaimHistory
{
    public int Id { get; set; }

    public int ClaimId { get; set; }

    public ClaimStatus FromStatus { get; set; }

    public ClaimStatus ToStatus { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string ChangedBy { get; set; } = "System";

    // Navigation
    public Claim Claim { get; set; } = null!;
}
