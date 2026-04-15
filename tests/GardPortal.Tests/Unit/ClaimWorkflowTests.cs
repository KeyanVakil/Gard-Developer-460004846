using FluentAssertions;
using GardPortal.Data;
using GardPortal.Models;
using GardPortal.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GardPortal.Tests.Unit;

public class ClaimWorkflowTests : IDisposable
{
    private readonly GardDbContext _db;
    private readonly ClaimService _sut;

    public ClaimWorkflowTests()
    {
        var options = new DbContextOptionsBuilder<GardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new GardDbContext(options);
        _sut = new ClaimService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<Vessel> CreateVesselAsync()
    {
        var vessel = new Vessel
        {
            Name         = "MV Test Vessel",
            ImoNumber    = "1234567",
            VesselType   = VesselType.BulkCarrier,
            FlagState    = "Norway",
            GrossTonnage = 50_000m,
            YearBuilt    = 2010,
        };
        _db.Vessels.Add(vessel);
        await _db.SaveChangesAsync();
        return vessel;
    }

    private async Task<Policy> CreatePolicyAsync(int vesselId, PolicyStatus status = PolicyStatus.Active)
    {
        var policy = new Policy
        {
            PolicyNumber  = $"POL-{Guid.NewGuid():N}"[..18],
            VesselId      = vesselId,
            CoverageType  = CoverageType.HullAndMachinery,
            Status        = status,
            StartDate     = DateTime.UtcNow.AddDays(-30),
            EndDate       = DateTime.UtcNow.AddDays(335),
            InsuredValue  = 5_000_000m,
            AnnualPremium = 20_000m,
        };
        _db.Policies.Add(policy);
        await _db.SaveChangesAsync();
        return policy;
    }

    private async Task<Claim> FileClaim(int policyId)
    {
        var claim = new Claim
        {
            PolicyId        = policyId,
            Category        = ClaimCategory.Collision,
            IncidentDate    = DateTime.UtcNow.AddDays(-5),
            Description     = "Collision incident during harbour approach.",
            EstimatedAmount = 150_000m,
        };
        return await _sut.CreateAsync(claim);
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_WithActivePolicy_GeneratesClaimNumber()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);

        var claim = await FileClaim(policy.Id);

        claim.ClaimNumber.Should().StartWith($"CLM-{DateTime.UtcNow.Year}-");
    }

    [Fact]
    public async Task Create_WithActivePolicy_SetsStatusToReported()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);

        var claim = await FileClaim(policy.Id);

        claim.Status.Should().Be(ClaimStatus.Reported);
    }

    [Fact]
    public async Task Create_WithActivePolicy_CreatesInitialHistoryEntry()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);

        var claim = await FileClaim(policy.Id);

        var history = await _db.ClaimHistories.Where(h => h.ClaimId == claim.Id).ToListAsync();
        history.Should().HaveCount(1);
        history[0].FromStatus.Should().Be(ClaimStatus.Reported);
        history[0].ToStatus.Should().Be(ClaimStatus.Reported);
    }

    [Fact]
    public async Task Create_AgainstDraftPolicy_ThrowsInvalidOperationException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id, PolicyStatus.Draft);

        var act = async () => await FileClaim(policy.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Active*");
    }

    [Fact]
    public async Task Create_AgainstExpiredPolicy_ThrowsInvalidOperationException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id, PolicyStatus.Expired);

        var act = async () => await FileClaim(policy.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Create_NonExistentPolicy_ThrowsKeyNotFoundException()
    {
        var act = async () => await FileClaim(9999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── TransitionStatusAsync (valid transitions) ──────────────────────────

    [Fact]
    public async Task Transition_ReportedToUnderReview_Succeeds()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);

        var updated = await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.UnderReview, "Starting review.");

        updated.Status.Should().Be(ClaimStatus.UnderReview);
    }

    [Fact]
    public async Task Transition_UnderReviewToApproved_Succeeds()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.UnderReview, null);

        var updated = await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Approved, "Liability confirmed.");

        updated.Status.Should().Be(ClaimStatus.Approved);
    }

    [Fact]
    public async Task Transition_ReportedToDenied_Succeeds()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);

        var updated = await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Denied, "Fraudulent claim.");

        updated.Status.Should().Be(ClaimStatus.Denied);
    }

    [Fact]
    public async Task Transition_CreatesHistoryEntry()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);

        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.UnderReview, "Review started.");

        var history = await _db.ClaimHistories.Where(h => h.ClaimId == claim.Id).ToListAsync();
        // Initial entry + one transition = 2
        history.Should().HaveCount(2);
        history.Last().FromStatus.Should().Be(ClaimStatus.Reported);
        history.Last().ToStatus.Should().Be(ClaimStatus.UnderReview);
    }

    // ── TransitionStatusAsync (invalid transitions) ────────────────────────

    [Fact]
    public async Task Transition_SettledToReported_ThrowsInvalidOperationException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.UnderReview, null);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Approved, null);
        await _sut.SettleAsync(claim.Id, 100_000m, null);

        var act = async () => await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Reported, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*transition*");
    }

    [Fact]
    public async Task Transition_DeniedToApproved_ThrowsInvalidOperationException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Denied, null);

        var act = async () => await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Approved, null);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Transition_ApprovedToReported_ThrowsInvalidOperationException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.UnderReview, null);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Approved, null);

        var act = async () => await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Reported, null);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── SettleAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task Settle_ApprovedClaim_SetsSettledAmountAndStatus()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.UnderReview, null);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Approved, null);

        var settled = await _sut.SettleAsync(claim.Id, 95_000m, "Agreed settlement.");

        settled.Status.Should().Be(ClaimStatus.Settled);
        settled.SettledAmount.Should().Be(95_000m);
    }

    [Fact]
    public async Task Settle_NonApprovedClaim_ThrowsInvalidOperationException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);

        var act = async () => await _sut.SettleAsync(claim.Id, 50_000m, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Approved*");
    }

    [Fact]
    public async Task Settle_ZeroAmount_ThrowsArgumentException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await CreatePolicyAsync(vessel.Id);
        var claim  = await FileClaim(policy.Id);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.UnderReview, null);
        await _sut.TransitionStatusAsync(claim.Id, ClaimStatus.Approved, null);

        var act = async () => await _sut.SettleAsync(claim.Id, 0m, null);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
