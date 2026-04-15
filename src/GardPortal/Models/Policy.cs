using System.ComponentModel.DataAnnotations;

namespace GardPortal.Models;

public class Policy
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string PolicyNumber { get; set; } = string.Empty;

    public int VesselId { get; set; }

    public CoverageType CoverageType { get; set; }

    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal InsuredValue { get; set; }

    public decimal AnnualPremium { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Vessel Vessel { get; set; } = null!;
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
