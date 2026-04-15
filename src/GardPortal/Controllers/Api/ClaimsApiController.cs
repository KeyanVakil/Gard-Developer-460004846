using GardPortal.DTOs;
using GardPortal.Models;
using GardPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardPortal.Controllers.Api;

[ApiController]
[Route("api/claims")]
public class ClaimsApiController : ControllerBase
{
    private readonly IClaimService _claimService;

    public ClaimsApiController(IClaimService claimService) => _claimService = claimService;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ClaimStatus? status = null,
        [FromQuery] ClaimCategory? category = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var (items, total) = await _claimService.GetAllAsync(page, pageSize, status, category, from, to);
        var dtos = items.Select(MapToDto).ToList();
        return Ok(PagedResponse<ClaimDto>.Success(dtos, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var claim = await _claimService.GetByIdAsync(id);
        if (claim is null) return NotFound(ApiResponse<ClaimDetailDto>.Failure(new ApiError("id", "Claim not found.")));

        var dto = new ClaimDetailDto
        {
            Id              = claim.Id,
            ClaimNumber     = claim.ClaimNumber,
            PolicyId        = claim.PolicyId,
            PolicyNumber    = claim.Policy?.PolicyNumber ?? string.Empty,
            VesselName      = claim.Policy?.Vessel?.Name ?? string.Empty,
            Category        = claim.Category.ToString(),
            Status          = claim.Status.ToString(),
            IncidentDate    = claim.IncidentDate,
            Description     = claim.Description,
            EstimatedAmount = claim.EstimatedAmount,
            SettledAmount   = claim.SettledAmount,
            Notes           = claim.Notes,
            CreatedAt       = claim.CreatedAt,
            UpdatedAt       = claim.UpdatedAt,
            History         = claim.History.OrderBy(h => h.ChangedAt).Select(h => new ClaimHistoryDto
            {
                Id         = h.Id,
                FromStatus = h.FromStatus.ToString(),
                ToStatus   = h.ToStatus.ToString(),
                Notes      = h.Notes,
                ChangedAt  = h.ChangedAt,
                ChangedBy  = h.ChangedBy,
            }).ToList(),
        };
        return Ok(ApiResponse<ClaimDetailDto>.Success(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClaimDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ClaimDto>.Failure(ModelStateErrors()));

        var claim = new Claim
        {
            PolicyId        = dto.PolicyId,
            Category        = dto.Category,
            IncidentDate    = dto.IncidentDate,
            Description     = dto.Description,
            EstimatedAmount = dto.EstimatedAmount,
            Notes           = dto.Notes,
        };

        try
        {
            var created = await _claimService.CreateAsync(claim);
            var result  = await _claimService.GetByIdAsync(created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ClaimDto>.Success(MapToDto(result!)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ClaimDto>.Failure(new ApiError("policyId", ex.Message)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ClaimDto>.Failure(new ApiError("policyId", ex.Message)));
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> TransitionStatus(int id, [FromBody] TransitionClaimStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ClaimDto>.Failure(ModelStateErrors()));

        try
        {
            var updated = await _claimService.TransitionStatusAsync(id, dto.NewStatus, dto.Notes);
            var result  = await _claimService.GetByIdAsync(updated.Id);
            return Ok(ApiResponse<ClaimDetailDto>.Success(MapToDetailDto(result!)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ClaimDto>.Failure(new ApiError("id", ex.Message)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ClaimDto>.Failure(new ApiError("status", ex.Message)));
        }
    }

    [HttpPatch("{id:int}/settle")]
    public async Task<IActionResult> Settle(int id, [FromBody] SettleClaimDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ClaimDto>.Failure(ModelStateErrors()));

        try
        {
            var settled = await _claimService.SettleAsync(id, dto.SettlementAmount, dto.Notes);
            var result  = await _claimService.GetByIdAsync(settled.Id);
            return Ok(ApiResponse<ClaimDetailDto>.Success(MapToDetailDto(result!)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ClaimDto>.Failure(new ApiError("id", ex.Message)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ClaimDto>.Failure(new ApiError("status", ex.Message)));
        }
    }

    private static ClaimDto MapToDto(Claim c) => new()
    {
        Id              = c.Id,
        ClaimNumber     = c.ClaimNumber,
        PolicyId        = c.PolicyId,
        PolicyNumber    = c.Policy?.PolicyNumber ?? string.Empty,
        VesselName      = c.Policy?.Vessel?.Name ?? string.Empty,
        Category        = c.Category.ToString(),
        Status          = c.Status.ToString(),
        IncidentDate    = c.IncidentDate,
        Description     = c.Description,
        EstimatedAmount = c.EstimatedAmount,
        SettledAmount   = c.SettledAmount,
        Notes           = c.Notes,
        CreatedAt       = c.CreatedAt,
        UpdatedAt       = c.UpdatedAt,
    };

    private static ClaimDetailDto MapToDetailDto(Claim c)
    {
        var dto = new ClaimDetailDto
        {
            Id              = c.Id,
            ClaimNumber     = c.ClaimNumber,
            PolicyId        = c.PolicyId,
            PolicyNumber    = c.Policy?.PolicyNumber ?? string.Empty,
            VesselName      = c.Policy?.Vessel?.Name ?? string.Empty,
            Category        = c.Category.ToString(),
            Status          = c.Status.ToString(),
            IncidentDate    = c.IncidentDate,
            Description     = c.Description,
            EstimatedAmount = c.EstimatedAmount,
            SettledAmount   = c.SettledAmount,
            Notes           = c.Notes,
            CreatedAt       = c.CreatedAt,
            UpdatedAt       = c.UpdatedAt,
            History         = c.History.OrderBy(h => h.ChangedAt).Select(h => new ClaimHistoryDto
            {
                Id         = h.Id,
                FromStatus = h.FromStatus.ToString(),
                ToStatus   = h.ToStatus.ToString(),
                Notes      = h.Notes,
                ChangedAt  = h.ChangedAt,
                ChangedBy  = h.ChangedBy,
            }).ToList(),
        };
        return dto;
    }

    private ApiError[] ModelStateErrors() =>
        ModelState.SelectMany(kv => kv.Value!.Errors.Select(e => new ApiError(kv.Key, e.ErrorMessage))).ToArray();
}
