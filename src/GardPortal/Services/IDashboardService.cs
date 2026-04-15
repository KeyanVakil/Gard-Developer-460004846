using GardPortal.DTOs;
using GardPortal.Models;

namespace GardPortal.Services;

public interface IDashboardService
{
    Task<SummaryDto> GetSummaryAsync(VesselType? vesselType = null);
    Task<List<CategoryCountDto>> GetClaimsByCategoryAsync(VesselType? vesselType = null);
    Task<List<CoverageCountDto>> GetPoliciesByCoverageAsync(VesselType? vesselType = null);
    Task<List<MonthlyTrendDto>> GetClaimsTrendAsync(VesselType? vesselType = null);
}
