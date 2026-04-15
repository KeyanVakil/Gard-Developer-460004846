using GardPortal.DTOs;
using GardPortal.Models;
using GardPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardPortal.Controllers.Api;

[ApiController]
[Route("api/policies")]
public class PoliciesApiController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PoliciesApiController(IPolicyService policyService) => _policyService = policyService;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] PolicyStatus? status = null,
        [FromQuery] CoverageType? coverageType = null,
        [FromQuery] string? vesselName = null)
    {
        var (items, total) = await _policyService.GetAllAsync(page, pageSize, status, coverageType, vesselName);
        var dtos = items.Select(MapToDto).ToList();
        return Ok(PagedResponse<PolicyDto>.Success(dtos, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var policy = await _policyService.GetByIdAsync(id);
        if (policy is null) return NotFound(ApiResponse<PolicyDetailDto>.Failure(new ApiError("id", "Policy not found.")));

        var dto = new PolicyDetailDto
        {
            Id                  = policy.Id,
            PolicyNumber        = policy.PolicyNumber,
            VesselId            = policy.VesselId,
            VesselName          = policy.Vessel.Name,
            VesselType          = policy.Vessel.VesselType.ToString(),
            CoverageType        = policy.CoverageType.ToString(),
            Status              = policy.Status.ToString(),
            StartDate           = policy.StartDate,
            EndDate             = policy.EndDate,
            InsuredValue        = policy.InsuredValue,
            AnnualPremium       = policy.AnnualPremium,
            Notes               = policy.Notes,
            CreatedAt           = policy.CreatedAt,
            UpdatedAt           = policy.UpdatedAt,
            ClaimsCount         = policy.Claims.Count,
            TotalClaimedAmount  = policy.Claims.Sum(c => c.EstimatedAmount),
            Claims              = policy.Claims.Select(MapClaimToDto).ToList(),
        };
        return Ok(ApiResponse<PolicyDetailDto>.Success(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePolicyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PolicyDto>.Failure(ModelStateErrors()));

        if (dto.EndDate <= dto.StartDate)
            return BadRequest(ApiResponse<PolicyDto>.Failure(new ApiError("EndDate", "End date must be after start date.")));

        var policy = new Policy
        {
            VesselId     = dto.VesselId,
            CoverageType = dto.CoverageType,
            StartDate    = dto.StartDate,
            EndDate      = dto.EndDate,
            InsuredValue = dto.InsuredValue,
            Notes        = dto.Notes,
        };

        try
        {
            var created = await _policyService.CreateAsync(policy);
            var result  = await _policyService.GetByIdAsync(created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<PolicyDto>.Success(MapToDto(result!)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PolicyDto>.Failure(new ApiError("vesselId", ex.Message)));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePolicyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PolicyDto>.Failure(ModelStateErrors()));

        if (dto.EndDate <= dto.StartDate)
            return BadRequest(ApiResponse<PolicyDto>.Failure(new ApiError("EndDate", "End date must be after start date.")));

        var policy = new Policy
        {
            Id           = id,
            VesselId     = dto.VesselId,
            CoverageType = dto.CoverageType,
            StartDate    = dto.StartDate,
            EndDate      = dto.EndDate,
            InsuredValue = dto.InsuredValue,
            Notes        = dto.Notes,
        };

        try
        {
            var updated = await _policyService.UpdateAsync(policy);
            var result  = await _policyService.GetByIdAsync(updated.Id);
            return Ok(ApiResponse<PolicyDto>.Success(MapToDto(result!)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PolicyDto>.Failure(new ApiError("id", ex.Message)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PolicyDto>.Failure(new ApiError("status", ex.Message)));
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> TransitionStatus(int id, [FromBody] TransitionPolicyStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PolicyDto>.Failure(ModelStateErrors()));

        try
        {
            var updated = await _policyService.TransitionStatusAsync(id, dto.NewStatus);
            var result  = await _policyService.GetByIdAsync(updated.Id);
            return Ok(ApiResponse<PolicyDto>.Success(MapToDto(result!)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PolicyDto>.Failure(new ApiError("id", ex.Message)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PolicyDto>.Failure(new ApiError("status", ex.Message)));
        }
    }

    private static PolicyDto MapToDto(Policy p) => new()
    {
        Id            = p.Id,
        PolicyNumber  = p.PolicyNumber,
        VesselId      = p.VesselId,
        VesselName    = p.Vessel?.Name ?? string.Empty,
        CoverageType  = p.CoverageType.ToString(),
        Status        = p.Status.ToString(),
        StartDate     = p.StartDate,
        EndDate       = p.EndDate,
        InsuredValue  = p.InsuredValue,
        AnnualPremium = p.AnnualPremium,
        Notes         = p.Notes,
        CreatedAt     = p.CreatedAt,
        UpdatedAt     = p.UpdatedAt,
    };

    private static ClaimDto MapClaimToDto(Claim c) => new()
    {
        Id              = c.Id,
        ClaimNumber     = c.ClaimNumber,
        PolicyId        = c.PolicyId,
        Category        = c.Category.ToString(),
        Status          = c.Status.ToString(),
        IncidentDate    = c.IncidentDate,
        Description     = c.Description,
        EstimatedAmount = c.EstimatedAmount,
        SettledAmount   = c.SettledAmount,
        CreatedAt       = c.CreatedAt,
        UpdatedAt       = c.UpdatedAt,
    };

    private ApiError[] ModelStateErrors() =>
        ModelState.SelectMany(kv => kv.Value!.Errors.Select(e => new ApiError(kv.Key, e.ErrorMessage))).ToArray();
}
