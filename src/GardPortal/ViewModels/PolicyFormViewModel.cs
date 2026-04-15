using GardPortal.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GardPortal.ViewModels;

public class PolicyFormViewModel
{
    public int Id { get; set; }
    public int VesselId { get; set; }
    public CoverageType CoverageType { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);
    public decimal InsuredValue { get; set; }
    public string? Notes { get; set; }

    public List<SelectListItem> Vessels { get; set; } = new();
}
