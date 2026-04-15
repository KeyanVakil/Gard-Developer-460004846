using GardPortal.Data;
using GardPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace GardPortal.Services;

public class PolicyService : IPolicyService
{
    private readonly GardDbContext _db;
    private readonly IPremiumCalculator _premiumCalculator;

    public PolicyService(GardDbContext db, IPremiumCalculator premiumCalculator)
    {
        _db = db;
        _premiumCalculator = premiumCalculator;
    }

    public async Task<(List<Policy> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize,
        PolicyStatus? status = null,
        CoverageType? coverageType = null,
        string? vesselName = null)
    {
        var query = _db.Policies.Include(p => p.Vessel).AsQueryable();

        if (status != null)       query = query.Where(p => p.Status == status);
        if (coverageType != null) query = query.Where(p => p.CoverageType == coverageType);
        if (!string.IsNullOrWhiteSpace(vesselName))
            query = query.Where(p => p.Vessel.Name.Contains(vesselName));

        query = query.OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, total);
    }

    public async Task<Policy?> GetByIdAsync(int id)
        => await _db.Policies
            .Include(p => p.Vessel)
            .Include(p => p.Claims)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Policy> CreateAsync(Policy policy)
    {
        var vessel = await _db.Vessels.FindAsync(policy.VesselId)
            ?? throw new KeyNotFoundException($"Vessel {policy.VesselId} not found.");

        policy.AnnualPremium = _premiumCalculator.CalculatePremium(
            policy.InsuredValue, policy.CoverageType, vessel.VesselType);

        // Generate policy number
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Policies.CountAsync(p => p.CreatedAt.Year == year) + 1;
        policy.PolicyNumber = $"POL-{year}-{count:D5}";

        policy.Status    = PolicyStatus.Draft;
        policy.CreatedAt = DateTime.UtcNow;
        policy.UpdatedAt = DateTime.UtcNow;

        _db.Policies.Add(policy);
        await _db.SaveChangesAsync();
        return policy;
    }

    public async Task<Policy> UpdateAsync(Policy policy)
    {
        var existing = await _db.Policies.Include(p => p.Vessel).FirstOrDefaultAsync(p => p.Id == policy.Id)
            ?? throw new KeyNotFoundException($"Policy {policy.Id} not found.");

        if (existing.Status != PolicyStatus.Draft)
            throw new InvalidOperationException("Only Draft policies can be updated.");

        existing.CoverageType  = policy.CoverageType;
        existing.StartDate     = policy.StartDate;
        existing.EndDate       = policy.EndDate;
        existing.InsuredValue  = policy.InsuredValue;
        existing.Notes         = policy.Notes;
        existing.AnnualPremium = _premiumCalculator.CalculatePremium(
            policy.InsuredValue, policy.CoverageType, existing.Vessel.VesselType);
        existing.UpdatedAt     = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<Policy> TransitionStatusAsync(int id, PolicyStatus newStatus)
    {
        var policy = await _db.Policies.FindAsync(id)
            ?? throw new KeyNotFoundException($"Policy {id} not found.");

        ValidateTransition(policy.Status, newStatus);

        policy.Status    = newStatus;
        policy.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return policy;
    }

    private static void ValidateTransition(PolicyStatus from, PolicyStatus to)
    {
        var valid = (from, to) switch
        {
            (PolicyStatus.Draft,   PolicyStatus.Active)    => true,
            (PolicyStatus.Draft,   PolicyStatus.Cancelled) => true,
            (PolicyStatus.Active,  PolicyStatus.Expired)   => true,
            (PolicyStatus.Active,  PolicyStatus.Cancelled) => true,
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException($"Invalid policy status transition: {from} → {to}.");
    }
}
