using System.ComponentModel.DataAnnotations;

namespace GardPortal.Models;

public class Vessel
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(7)]
    public string ImoNumber { get; set; } = string.Empty;

    public VesselType VesselType { get; set; }

    [Required, MaxLength(100)]
    public string FlagState { get; set; } = string.Empty;

    public decimal GrossTonnage { get; set; }

    public int YearBuilt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
