using GardPortal.DTOs;
using GardPortal.Models;
using GardPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardPortal.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
public class DashboardApiController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardApiController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] VesselType? vesselType = null)
    {
        var summary = await _dashboardService.GetSummaryAsync(vesselType);
        return Ok(ApiResponse<SummaryDto>.Success(summary));
    }

    [HttpGet("claims-by-category")]
    public async Task<IActionResult> GetClaimsByCategory([FromQuery] VesselType? vesselType = null)
    {
        var data = await _dashboardService.GetClaimsByCategoryAsync(vesselType);
        return Ok(ApiResponse<List<CategoryCountDto>>.Success(data));
    }

    [HttpGet("policies-by-coverage")]
    public async Task<IActionResult> GetPoliciesByCoverage([FromQuery] VesselType? vesselType = null)
    {
        var data = await _dashboardService.GetPoliciesByCoverageAsync(vesselType);
        return Ok(ApiResponse<List<CoverageCountDto>>.Success(data));
    }

    [HttpGet("claims-trend")]
    public async Task<IActionResult> GetClaimsTrend([FromQuery] VesselType? vesselType = null)
    {
        var data = await _dashboardService.GetClaimsTrendAsync(vesselType);
        return Ok(ApiResponse<List<MonthlyTrendDto>>.Success(data));
    }
}
