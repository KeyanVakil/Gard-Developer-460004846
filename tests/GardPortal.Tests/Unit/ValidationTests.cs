using FluentAssertions;
using GardPortal.Data;
using GardPortal.Models;
using GardPortal.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GardPortal.Tests.Unit;

public class ValidationTests : IDisposable
{
    private readonly GardDbContext _db;
    private readonly VesselService _vesselSut;

    public ValidationTests()
    {
        var options = new DbContextOptionsBuilder<GardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new GardDbContext(options);
        _vesselSut = new VesselService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ── Helpers ────────────────────────────────────────────────────────────

    private Vessel BuildVessel(string imo = "1234567") => new()
    {
        Name         = "MV Test",
        ImoNumber    = imo,
        VesselType   = VesselType.BulkCarrier,
        FlagState    = "Norway",
        GrossTonnage = 20_000m,
        YearBuilt    = 2005,
    };

    private async Task<Policy> AttachPolicyAsync(int vesselId, PolicyStatus status)
    {
        var policy = new Policy
        {
            PolicyNumber  = $"POL-{Guid.NewGuid():N}"[..18],
            VesselId      = vesselId,
            CoverageType  = CoverageType.Cargo,
            Status        = status,
            StartDate     = DateTime.UtcNow.AddDays(-30),
            EndDate       = DateTime.UtcNow.AddDays(335),
            InsuredValue  = 1_000_000m,
            AnnualPremium = 1_500m,
        };
        _db.Policies.Add(policy);
        await _db.SaveChangesAsync();
        return policy;
    }

    // ── Vessel deletion ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteVessel_WithActivePolicy_ThrowsInvalidOperationException()
    {
        var vessel = await _vesselSut.CreateAsync(BuildVessel("1000001"));
        await AttachPolicyAsync(vessel.Id, PolicyStatus.Active);

        var act = async () => await _vesselSut.DeleteAsync(vessel.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*active*");
    }

    [Fact]
    public async Task DeleteVessel_WithOnlyDraftPolicies_Succeeds()
    {
        var vessel = await _vesselSut.CreateAsync(BuildVessel("1000002"));
        await AttachPolicyAsync(vessel.Id, PolicyStatus.Draft);

        var act = async () => await _vesselSut.DeleteAsync(vessel.Id);

        await act.Should().NotThrowAsync();
        var deleted = await _db.Vessels.FindAsync(vessel.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteVessel_WithExpiredPolicies_Succeeds()
    {
        var vessel = await _vesselSut.CreateAsync(BuildVessel("1000003"));
        await AttachPolicyAsync(vessel.Id, PolicyStatus.Expired);

        var act = async () => await _vesselSut.DeleteAsync(vessel.Id);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteVessel_NoPolicies_Succeeds()
    {
        var vessel = await _vesselSut.CreateAsync(BuildVessel("1000004"));

        await _vesselSut.DeleteAsync(vessel.Id);

        var deleted = await _db.Vessels.FindAsync(vessel.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteVessel_NotFound_ThrowsKeyNotFoundException()
    {
        var act = async () => await _vesselSut.DeleteAsync(9999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
