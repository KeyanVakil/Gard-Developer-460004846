using FluentAssertions;
using GardPortal.Data;
using GardPortal.Models;
using GardPortal.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GardPortal.Tests.Unit;

public class PolicyWorkflowTests : IDisposable
{
    private readonly GardDbContext _db;
    private readonly Mock<IPremiumCalculator> _calculatorMock;
    private readonly PolicyService _sut;

    public PolicyWorkflowTests()
    {
        var options = new DbContextOptionsBuilder<GardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new GardDbContext(options);

        _calculatorMock = new Mock<IPremiumCalculator>();
        _calculatorMock
            .Setup(c => c.CalculatePremium(It.IsAny<decimal>(), It.IsAny<CoverageType>(), It.IsAny<VesselType>()))
            .Returns(20_000m);

        _sut = new PolicyService(_db, _calculatorMock.Object);
    }

    public void Dispose() => _db.Dispose();

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<Vessel> CreateVesselAsync(string imo = "9876543")
    {
        var vessel = new Vessel
        {
            Name         = "MV Test Ship",
            ImoNumber    = imo,
            VesselType   = VesselType.ContainerShip,
            FlagState    = "Norway",
            GrossTonnage = 30_000m,
            YearBuilt    = 2015,
        };
        _db.Vessels.Add(vessel);
        await _db.SaveChangesAsync();
        return vessel;
    }

    private Policy BuildPolicy(int vesselId) => new()
    {
        VesselId      = vesselId,
        CoverageType  = CoverageType.HullAndMachinery,
        StartDate     = DateTime.UtcNow,
        EndDate       = DateTime.UtcNow.AddYears(1),
        InsuredValue  = 5_000_000m,
    };

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_GeneratesPolicyNumber()
    {
        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));

        policy.PolicyNumber.Should().StartWith($"POL-{DateTime.UtcNow.Year}-");
    }

    [Fact]
    public async Task Create_SetsDraftStatus()
    {
        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));

        policy.Status.Should().Be(PolicyStatus.Draft);
    }

    [Fact]
    public async Task Create_CallsPremiumCalculator()
    {
        var vessel = await CreateVesselAsync();
        await _sut.CreateAsync(BuildPolicy(vessel.Id));

        _calculatorMock.Verify(c => c.CalculatePremium(
            5_000_000m,
            CoverageType.HullAndMachinery,
            VesselType.ContainerShip), Times.Once);
    }

    [Fact]
    public async Task Create_SetsCalculatedPremiumOnPolicy()
    {
        _calculatorMock
            .Setup(c => c.CalculatePremium(It.IsAny<decimal>(), It.IsAny<CoverageType>(), It.IsAny<VesselType>()))
            .Returns(18_500m);

        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));

        policy.AnnualPremium.Should().Be(18_500m);
    }

    [Fact]
    public async Task Create_NonExistentVessel_ThrowsKeyNotFoundException()
    {
        var act = async () => await _sut.CreateAsync(BuildPolicy(9999));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── Status transitions ─────────────────────────────────────────────────

    [Fact]
    public async Task Transition_DraftToActive_Succeeds()
    {
        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));

        var updated = await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Active);

        updated.Status.Should().Be(PolicyStatus.Active);
    }

    [Fact]
    public async Task Transition_DraftToCancelled_Succeeds()
    {
        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));

        var updated = await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Cancelled);

        updated.Status.Should().Be(PolicyStatus.Cancelled);
    }

    [Fact]
    public async Task Transition_ActiveToExpired_Succeeds()
    {
        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));
        await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Active);

        var updated = await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Expired);

        updated.Status.Should().Be(PolicyStatus.Expired);
    }

    [Fact]
    public async Task Transition_ExpiredToActive_ThrowsInvalidOperationException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));
        await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Active);
        await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Expired);

        var act = async () => await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Active);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*transition*");
    }

    [Fact]
    public async Task Transition_CancelledToDraft_ThrowsInvalidOperationException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));
        await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Cancelled);

        var act = async () => await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Draft);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_DraftPolicy_RecalculatesPremium()
    {
        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));

        _calculatorMock
            .Setup(c => c.CalculatePremium(7_000_000m, CoverageType.Cargo, VesselType.ContainerShip))
            .Returns(35_000m);

        var updateRequest = new Policy
        {
            Id           = policy.Id,
            CoverageType = CoverageType.Cargo,
            StartDate    = policy.StartDate,
            EndDate      = policy.EndDate,
            InsuredValue = 7_000_000m,
        };

        var updated = await _sut.UpdateAsync(updateRequest);

        updated.AnnualPremium.Should().Be(35_000m);
        updated.InsuredValue.Should().Be(7_000_000m);
    }

    [Fact]
    public async Task Update_ActivePolicy_ThrowsInvalidOperationException()
    {
        var vessel = await CreateVesselAsync();
        var policy = await _sut.CreateAsync(BuildPolicy(vessel.Id));
        await _sut.TransitionStatusAsync(policy.Id, PolicyStatus.Active);

        var updateRequest = new Policy { Id = policy.Id, InsuredValue = 9_000_000m };
        var act = async () => await _sut.UpdateAsync(updateRequest);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Draft*");
    }
}
