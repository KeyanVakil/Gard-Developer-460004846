using System.ComponentModel.DataAnnotations;
using GardPortal.Models;

namespace GardPortal.DTOs;

public class CreateVesselDto
{
    [Required(ErrorMessage = "Vessel name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "IMO Number is required.")]
    [RegularExpression(@"^\d{7}$", ErrorMessage = "IMO Number must be exactly 7 digits.")]
    public string ImoNumber { get; set; } = string.Empty;

    [Required]
    public VesselType VesselType { get; set; }

    [Required(ErrorMessage = "Flag state is required.")]
    [MaxLength(100)]
    public string FlagState { get; set; } = string.Empty;

    [Range(1, 500000, ErrorMessage = "Gross tonnage must be between 1 and 500,000.")]
    public decimal GrossTonnage { get; set; }

    [Range(1900, 2100, ErrorMessage = "Year built must be between 1900 and 2100.")]
    public int YearBuilt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class VesselDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImoNumber { get; set; } = string.Empty;
    public string VesselType { get; set; } = string.Empty;
    public string FlagState { get; set; } = string.Empty;
    public decimal GrossTonnage { get; set; }
    public int YearBuilt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class VesselDetailDto : VesselDto
{
    public int ActivePoliciesCount { get; set; }
    public int TotalClaimsCount { get; set; }
    public decimal TotalInsuredValue { get; set; }
}
