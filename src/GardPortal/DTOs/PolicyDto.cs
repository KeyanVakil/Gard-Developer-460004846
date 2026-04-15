using System.ComponentModel.DataAnnotations;
using GardPortal.Models;

namespace GardPortal.DTOs;

public class CreatePolicyDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A vessel must be selected.")]
    public int VesselId { get; set; }

    [Required]
    public CoverageType CoverageType { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Insured value must be positive.")]
    public decimal InsuredValue { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult("End date must be after start date.", new[] { nameof(EndDate) });
    }
}

public class PolicyDto
{
    public int Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public int VesselId { get; set; }
    public string VesselName { get; set; } = string.Empty;
    public string CoverageType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal InsuredValue { get; set; }
    public decimal AnnualPremium { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PolicyDetailDto : PolicyDto
{
    public string VesselType { get; set; } = string.Empty;
    public int ClaimsCount { get; set; }
    public decimal TotalClaimedAmount { get; set; }
    public List<ClaimDto> Claims { get; set; } = new();
}

public class TransitionPolicyStatusDto
{
    [Required]
    public PolicyStatus NewStatus { get; set; }
}
