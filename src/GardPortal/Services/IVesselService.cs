using GardPortal.Models;

namespace GardPortal.Services;

public interface IVesselService
{
    Task<(List<Vessel> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? sortBy = null);
    Task<Vessel?> GetByIdAsync(int id);
    Task<Vessel> CreateAsync(Vessel vessel);
    Task<Vessel> UpdateAsync(Vessel vessel);
    Task DeleteAsync(int id);
    Task<int> GetVesselCountAsync();
}
