using GardPortal.Data;
using GardPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace GardPortal.Services;

public class VesselService : IVesselService
{
    private readonly GardDbContext _db;

    public VesselService(GardDbContext db) => _db = db;

    public async Task<(List<Vessel> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? sortBy = null)
    {
        var query = _db.Vessels.AsQueryable();

        query = sortBy?.ToLowerInvariant() switch
        {
            "name"         => query.OrderBy(v => v.Name),
            "imonumber"    => query.OrderBy(v => v.ImoNumber),
            "vesseltype"   => query.OrderBy(v => v.VesselType),
            "flagstate"    => query.OrderBy(v => v.FlagState),
            "grosstonnage" => query.OrderBy(v => v.GrossTonnage),
            "yearbuilt"    => query.OrderBy(v => v.YearBuilt),
            _              => query.OrderBy(v => v.Name),
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Vessel?> GetByIdAsync(int id)
        => await _db.Vessels
            .Include(v => v.Policies)
                .ThenInclude(p => p.Claims)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<Vessel> CreateAsync(Vessel vessel)
    {
        vessel.CreatedAt = DateTime.UtcNow;
        vessel.UpdatedAt = DateTime.UtcNow;
        _db.Vessels.Add(vessel);
        await _db.SaveChangesAsync();
        return vessel;
    }

    public async Task<Vessel> UpdateAsync(Vessel vessel)
    {
        var existing = await _db.Vessels.FindAsync(vessel.Id)
            ?? throw new KeyNotFoundException($"Vessel {vessel.Id} not found.");

        existing.Name         = vessel.Name;
        existing.ImoNumber    = vessel.ImoNumber;
        existing.VesselType   = vessel.VesselType;
        existing.FlagState    = vessel.FlagState;
        existing.GrossTonnage = vessel.GrossTonnage;
        existing.YearBuilt    = vessel.YearBuilt;
        existing.Notes        = vessel.Notes;
        existing.UpdatedAt    = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var vessel = await _db.Vessels
            .Include(v => v.Policies)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Vessel {id} not found.");

        var hasActivePolicies = vessel.Policies.Any(p => p.Status == PolicyStatus.Active);
        if (hasActivePolicies)
            throw new InvalidOperationException("Cannot delete a vessel with active policies.");

        _db.Vessels.Remove(vessel);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetVesselCountAsync()
        => await _db.Vessels.CountAsync();
}
