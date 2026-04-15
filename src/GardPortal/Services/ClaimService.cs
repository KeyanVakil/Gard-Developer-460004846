using GardPortal.Data;
using GardPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace GardPortal.Services;

public class ClaimService : IClaimService
{
    private readonly GardDbContext _db;

    public ClaimService(GardDbContext db) => _db = db;

    public async Task<(List<Claim> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize,
        ClaimStatus? status, ClaimCategory? category,
        DateTime? from, DateTime? to)
    {
        var query = _db.Claims.Include(c => c.Policy).ThenInclude(p => p.Vessel).AsQueryable();

        if (status   != null) query = query.Where(c => c.Status   == status);
        if (category != null) query = query.Where(c => c.Category == category);
        if (from     != null) query = query.Where(c => c.IncidentDate >= from.Value);
        if (to       != null) query = query.Where(c => c.IncidentDate <= to.Value);

        query = query.OrderByDescending(c => c.IncidentDate);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, total);
    }

    public async Task<Claim?> GetByIdAsync(int id)
        => await _db.Claims
            .Include(c => c.Policy).ThenInclude(p => p.Vessel)
            .Include(c => c.History)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Claim> CreateAsync(Claim claim)
    {
        var policy = await _db.Policies.FindAsync(claim.PolicyId)
            ?? throw new KeyNotFoundException($"Policy {claim.PolicyId} not found.");

        if (policy.Status != PolicyStatus.Active)
            throw new InvalidOperationException("Claims can only be filed against Active policies.");

        var year  = DateTime.UtcNow.Year;
        var count = await _db.Claims.CountAsync(c => c.CreatedAt.Year == year) + 1;
        claim.ClaimNumber = $"CLM-{year}-{count:D5}";

        claim.Status    = ClaimStatus.Reported;
        claim.CreatedAt = DateTime.UtcNow;
        claim.UpdatedAt = DateTime.UtcNow;

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();

        // Initial history entry
        _db.ClaimHistories.Add(new ClaimHistory
        {
            ClaimId    = claim.Id,
            FromStatus = ClaimStatus.Reported,
            ToStatus   = ClaimStatus.Reported,
            Notes      = "Claim filed and registered.",
            ChangedAt  = claim.CreatedAt,
            ChangedBy  = "System"
        });
        await _db.SaveChangesAsync();

        return claim;
    }

    public async Task<Claim> TransitionStatusAsync(int id, ClaimStatus newStatus, string? notes)
    {
        var claim = await _db.Claims.FindAsync(id)
            ?? throw new KeyNotFoundException($"Claim {id} not found.");

        ValidateTransition(claim.Status, newStatus);

        var from = claim.Status;
        claim.Status    = newStatus;
        claim.UpdatedAt = DateTime.UtcNow;

        _db.ClaimHistories.Add(new ClaimHistory
        {
            ClaimId    = claim.Id,
            FromStatus = from,
            ToStatus   = newStatus,
            Notes      = notes,
            ChangedAt  = DateTime.UtcNow,
            ChangedBy  = "System"
        });

        await _db.SaveChangesAsync();
        return claim;
    }

    public async Task<Claim> SettleAsync(int id, decimal settlementAmount, string? notes)
    {
        var claim = await _db.Claims.FindAsync(id)
            ?? throw new KeyNotFoundException($"Claim {id} not found.");

        if (claim.Status != ClaimStatus.Approved)
            throw new InvalidOperationException("Only Approved claims can be settled.");

        if (settlementAmount <= 0)
            throw new ArgumentException("Settlement amount must be positive.");

        var from = claim.Status;
        claim.Status         = ClaimStatus.Settled;
        claim.SettledAmount  = settlementAmount;
        claim.UpdatedAt      = DateTime.UtcNow;

        _db.ClaimHistories.Add(new ClaimHistory
        {
            ClaimId    = claim.Id,
            FromStatus = from,
            ToStatus   = ClaimStatus.Settled,
            Notes      = notes ?? $"Settled for {settlementAmount:C2}.",
            ChangedAt  = DateTime.UtcNow,
            ChangedBy  = "System"
        });

        await _db.SaveChangesAsync();
        return claim;
    }

    private static void ValidateTransition(ClaimStatus from, ClaimStatus to)
    {
        var valid = (from, to) switch
        {
            (ClaimStatus.Reported,    ClaimStatus.UnderReview) => true,
            (ClaimStatus.UnderReview, ClaimStatus.Approved)    => true,
            (ClaimStatus.Approved,    ClaimStatus.Settled)     => true,
            // Denied from any non-terminal
            (ClaimStatus.Reported,    ClaimStatus.Denied)      => true,
            (ClaimStatus.UnderReview, ClaimStatus.Denied)      => true,
            (ClaimStatus.Approved,    ClaimStatus.Denied)      => true,
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException($"Invalid claim status transition: {from} → {to}.");
    }
}
