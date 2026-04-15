using GardPortal.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GardPortal.ViewModels;

public class ClaimDetailViewModel
{
    public Claim Claim { get; set; } = null!;
    public List<SelectListItem> NextStatuses { get; set; } = new();
    public ClaimStatus? SelectedStatus { get; set; }
    public string? TransitionNotes { get; set; }
}
