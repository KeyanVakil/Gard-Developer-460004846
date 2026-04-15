using GardPortal.DTOs;
using GardPortal.Models;
using GardPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardPortal.Controllers.Api;

[ApiController]
[Route("api/vessels")]
public class VesselsApiController : ControllerBase
{
    private readonly IVesselService _vesselService;

    public VesselsApiController(IVesselService vesselService) => _vesselService = vesselService;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null)
    {
        var (items, total) = await _vesselService.GetAllAsync(page, pageSize, sortBy);
        var dtos = items.Select(MapToDto).ToList();
        return Ok(PagedResponse<VesselDto>.Success(dtos, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vessel = await _vesselService.GetByIdAsync(id);
        if (vessel is null) return NotFound(ApiResponse<VesselDetailDto>.Failure(new ApiError("id", "Vessel not found.")));

        var dto = new VesselDetailDto
        {
            Id                  = vessel.Id,
            Name                = vessel.Name,
            ImoNumber           = vessel.ImoNumber,
            VesselType          = vessel.VesselType.ToString(),
            FlagState           = vessel.FlagState,
            GrossTonnage        = vessel.GrossTonnage,
            YearBuilt           = vessel.YearBuilt,
            Notes               = vessel.Notes,
            CreatedAt           = vessel.CreatedAt,
            UpdatedAt           = vessel.UpdatedAt,
            ActivePoliciesCount = vessel.Policies.Count(p => p.Status == PolicyStatus.Active),
            TotalClaimsCount    = vessel.Policies.SelectMany(p => p.Claims).Count(),
            TotalInsuredValue   = vessel.Policies.Where(p => p.Status == PolicyStatus.Active).Sum(p => p.InsuredValue),
        };
        return Ok(ApiResponse<VesselDetailDto>.Success(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVesselDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<VesselDto>.Failure(ModelStateErrors()));

        var vessel = new Vessel
        {
            Name         = dto.Name,
            ImoNumber    = dto.ImoNumber,
            VesselType   = dto.VesselType,
            FlagState    = dto.FlagState,
            GrossTonnage = dto.GrossTonnage,
            YearBuilt    = dto.YearBuilt,
            Notes        = dto.Notes,
        };

        try
        {
            var created = await _vesselService.CreateAsync(vessel);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<VesselDto>.Success(MapToDto(created)));
        }
        catch (Exception ex)
        {
            return Conflict(ApiResponse<VesselDto>.Failure(new ApiError("", ex.Message)));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateVesselDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<VesselDto>.Failure(ModelStateErrors()));

        var vessel = new Vessel
        {
            Id           = id,
            Name         = dto.Name,
            ImoNumber    = dto.ImoNumber,
            VesselType   = dto.VesselType,
            FlagState    = dto.FlagState,
            GrossTonnage = dto.GrossTonnage,
            YearBuilt    = dto.YearBuilt,
            Notes        = dto.Notes,
        };

        try
        {
            var updated = await _vesselService.UpdateAsync(vessel);
            return Ok(ApiResponse<VesselDto>.Success(MapToDto(updated)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VesselDto>.Failure(new ApiError("id", ex.Message)));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _vesselService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Success(new { message = "Vessel deleted." }));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Failure(new ApiError("id", ex.Message)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Failure(new ApiError("", ex.Message)));
        }
    }

    private static VesselDto MapToDto(Vessel v) => new()
    {
        Id           = v.Id,
        Name         = v.Name,
        ImoNumber    = v.ImoNumber,
        VesselType   = v.VesselType.ToString(),
        FlagState    = v.FlagState,
        GrossTonnage = v.GrossTonnage,
        YearBuilt    = v.YearBuilt,
        Notes        = v.Notes,
        CreatedAt    = v.CreatedAt,
        UpdatedAt    = v.UpdatedAt,
    };

    private ApiError[] ModelStateErrors() =>
        ModelState.SelectMany(kv => kv.Value!.Errors.Select(e => new ApiError(kv.Key, e.ErrorMessage))).ToArray();
}
