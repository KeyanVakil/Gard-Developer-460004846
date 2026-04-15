using GardPortal.Models;

namespace GardPortal.Services;

public interface IPolicyService
{
    Task<(List<Policy> Items, int TotalCount)> GetAllAsync(int page, int pageSize, PolicyStatus? status = null, CoverageType? coverageType = null, string? vesselName = null);
    Task<Policy?> GetByIdAsync(int id);
    Task<Policy> CreateAsync(Policy policy);
    Task<Policy> UpdateAsync(Policy policy);
    Task<Policy> TransitionStatusAsync(int id, PolicyStatus newStatus);
}
