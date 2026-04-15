using GardPortal.Data;
using GardPortal.DTOs;
using GardPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace GardPortal.Services;

public class DashboardService : IDashboardService
{
    private readonly GardDbContext _db;

    public DashboardService(GardDbContext db) => _db = db;

    public async Task<SummaryDto> GetSummaryAsync(VesselType? vesselType = null)
    {
        var policyQuery = _db.Policies.Include(p => p.Vessel).AsQueryable();
        var claimQuery  = _db.Claims.Include(c => c.Policy).ThenInclude(p => p.Vessel).AsQueryable();

        if (vesselType != null)
        {
            policyQuery = policyQuery.Where(p => p.Vessel.VesselType == vesselType);
            claimQuery  = claimQuery.Where(c => c.Policy.Vessel.VesselType == vesselType);
        }

        var activePolicies = await policyQuery.Where(p => p.Status == PolicyStatus.Active).ToListAsync();
        var openClaims     = await claimQuery.Where(c => c.Status != ClaimStatus.Settled && c.Status != ClaimStatus.Denied).ToListAsync();

        return new SummaryDto
        {
            TotalActivePolicies    = activePolicies.Count,
            TotalInsuredValue      = activePolicies.Sum(p => p.InsuredValue),
            TotalAnnualPremium     = activePolicies.Sum(p => p.AnnualPremium),
            OpenClaimsCount        = openClaims.Count,
            OutstandingClaimsValue = openClaims.Sum(c => c.EstimatedAmount),
        };
    }

    public async Task<List<CategoryCountDto>> GetClaimsByCategoryAsync(VesselType? vesselType = null)
    {
        var query = _db.Claims.Include(c => c.Policy).ThenInclude(p => p.Vessel).AsQueryable();

        if (vesselType != null)
            query = query.Where(c => c.Policy.Vessel.VesselType == vesselType);

        var result = await query
            .GroupBy(c => c.Category)
            .Select(g => new CategoryCountDto
            {
                Category = g.Key.ToString(),
                Count    = g.Count(),
                TotalEstimatedAmount = g.Sum(c => c.EstimatedAmount)
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return result;
    }

    public async Task<List<CoverageCountDto>> GetPoliciesByCoverageAsync(VesselType? vesselType = null)
    {
        var query = _db.Policies.Include(p => p.Vessel).AsQueryable();

        if (vesselType != null)
            query = query.Where(p => p.Vessel.VesselType == vesselType);

        var result = await query
            .Where(p => p.Status == PolicyStatus.Active)
            .GroupBy(p => p.CoverageType)
            .Select(g => new CoverageCountDto
            {
                CoverageType     = g.Key.ToString(),
                Count            = g.Count(),
                TotalInsuredValue = g.Sum(p => p.InsuredValue)
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return result;
    }

    public async Task<List<MonthlyTrendDto>> GetClaimsTrendAsync(VesselType? vesselType = null)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-12);

        var query = _db.Claims.Include(c => c.Policy).ThenInclude(p => p.Vessel).AsQueryable();

        if (vesselType != null)
            query = query.Where(c => c.Policy.Vessel.VesselType == vesselType);

        var claims = await query
            .Where(c => c.CreatedAt >= cutoff)
            .ToListAsync();

        // Build 12-month trailing list
        var months = Enumerable.Range(0, 12)
            .Select(i => DateTime.UtcNow.AddMonths(-11 + i))
            .ToList();

        return months.Select(m => new MonthlyTrendDto
        {
            Year  = m.Year,
            Month = m.Month,
            Label = m.ToString("MMM yyyy"),
            Count = claims.Count(c => c.CreatedAt.Year == m.Year && c.CreatedAt.Month == m.Month),
            TotalEstimatedAmount = claims
                .Where(c => c.CreatedAt.Year == m.Year && c.CreatedAt.Month == m.Month)
                .Sum(c => c.EstimatedAmount)
        }).ToList();
    }
}
