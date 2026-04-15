using GardPortal.Models;

namespace GardPortal.ViewModels;

public class VesselListViewModel
{
    public List<Vessel> Vessels { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? SortBy { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
