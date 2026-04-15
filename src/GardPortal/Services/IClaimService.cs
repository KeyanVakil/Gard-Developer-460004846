using GardPortal.Models;

namespace GardPortal.Services;

public interface IClaimService
{
    Task<(List<Claim> Items, int TotalCount)> GetAllAsync(int page, int pageSize, ClaimStatus? status, ClaimCategory? category, DateTime? from, DateTime? to);
    Task<Claim?> GetByIdAsync(int id);
    Task<Claim> CreateAsync(Claim claim);
    Task<Claim> TransitionStatusAsync(int id, ClaimStatus newStatus, string? notes);
    Task<Claim> SettleAsync(int id, decimal settlementAmount, string? notes);
}
