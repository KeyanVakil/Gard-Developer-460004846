using System.ComponentModel.DataAnnotations;
using GardPortal.Models;

namespace GardPortal.DTOs;

public class CreateClaimDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A policy must be selected.")]
    public int PolicyId { get; set; }

    [Required]
    public ClaimCategory Category { get; set; }

    [Required]
    public DateTime IncidentDate { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Estimated amount must be positive.")]
    public decimal EstimatedAmount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class ClaimDto
{
    public int Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string VesselName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedAmount { get; set; }
    public decimal? SettledAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ClaimHistoryDto
{
    public int Id { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}

public class ClaimDetailDto : ClaimDto
{
    public List<ClaimHistoryDto> History { get; set; } = new();
}

public class TransitionClaimStatusDto
{
    [Required]
    public ClaimStatus NewStatus { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class SettleClaimDto
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Settlement amount must be positive.")]
    public decimal SettlementAmount { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
