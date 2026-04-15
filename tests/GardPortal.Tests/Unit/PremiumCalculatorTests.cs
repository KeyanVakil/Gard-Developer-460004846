using FluentAssertions;
using GardPortal.Models;
using GardPortal.Services;
using Xunit;

namespace GardPortal.Tests.Unit;

public class PremiumCalculatorTests
{
    private readonly PremiumCalculator _sut = new();

    // ── P&I (base rate 0.0025) ──────────────────────────────────────────────

    [Fact]
    public void PandI_BulkCarrier_ReturnsCorrectPremium()
    {
        // 1_000_000 * 0.0025 * 1.0 = 2,500.00
        var result = _sut.CalculatePremium(1_000_000m, CoverageType.ProtectionAndIndemnity, VesselType.BulkCarrier);
        result.Should().Be(2_500.00m);
    }

    [Fact]
    public void PandI_Tanker_AppliesHigherFactor()
    {
        // 1_000_000 * 0.0025 * 1.3 = 3,250.00
        var result = _sut.CalculatePremium(1_000_000m, CoverageType.ProtectionAndIndemnity, VesselType.Tanker);
        result.Should().Be(3_250.00m);
    }

    // ── H&M (base rate 0.0040) ─────────────────────────────────────────────

    [Fact]
    public void HullAndMachinery_ContainerShip_ReturnsCorrectPremium()
    {
        // 500_000 * 0.0040 * 1.1 = 2,200.00
        var result = _sut.CalculatePremium(500_000m, CoverageType.HullAndMachinery, VesselType.ContainerShip);
        result.Should().Be(2_200.00m);
    }

    [Fact]
    public void HullAndMachinery_Offshore_AppliesHighestFactor()
    {
        // 500_000 * 0.0040 * 1.4 = 2,800.00
        var result = _sut.CalculatePremium(500_000m, CoverageType.HullAndMachinery, VesselType.Offshore);
        result.Should().Be(2_800.00m);
    }

    // ── Cargo (base rate 0.0015) ────────────────────────────────────────────

    [Fact]
    public void Cargo_RoRo_ReturnsCorrectPremium()
    {
        // 2_000_000 * 0.0015 * 1.05 = 3,150.00
        var result = _sut.CalculatePremium(2_000_000m, CoverageType.Cargo, VesselType.RoRo);
        result.Should().Be(3_150.00m);
    }

    [Fact]
    public void Cargo_GeneralCargo_AppliesSubUnitFactor()
    {
        // 2_000_000 * 0.0015 * 0.95 = 2,850.00
        var result = _sut.CalculatePremium(2_000_000m, CoverageType.Cargo, VesselType.GeneralCargo);
        result.Should().Be(2_850.00m);
    }

    // ── LossOfHire (base rate 0.0020) ─────────────────────────────────────

    [Fact]
    public void LossOfHire_PassengerFerry_ReturnsCorrectPremium()
    {
        // 750_000 * 0.0020 * 1.2 = 1,800.00
        var result = _sut.CalculatePremium(750_000m, CoverageType.LossOfHire, VesselType.PassengerFerry);
        result.Should().Be(1_800.00m);
    }

    [Fact]
    public void LossOfHire_BulkCarrier_ReturnsCorrectPremium()
    {
        // 750_000 * 0.0020 * 1.0 = 1,500.00
        var result = _sut.CalculatePremium(750_000m, CoverageType.LossOfHire, VesselType.BulkCarrier);
        result.Should().Be(1_500.00m);
    }

    // ── All vessel type factors ────────────────────────────────────────────

    // insuredValue=500_000, coverage=HullAndMachinery (rate 0.0040)
    // expected = 500_000 * 0.0040 * vesselFactor
    [Theory]
    [InlineData(VesselType.BulkCarrier,    2000.00)]   // * 1.00
    [InlineData(VesselType.Tanker,         2600.00)]   // * 1.30
    [InlineData(VesselType.ContainerShip,  2200.00)]   // * 1.10
    [InlineData(VesselType.RoRo,           2100.00)]   // * 1.05
    [InlineData(VesselType.GeneralCargo,   1900.00)]   // * 0.95
    [InlineData(VesselType.Offshore,       2800.00)]   // * 1.40
    [InlineData(VesselType.PassengerFerry, 2400.00)]   // * 1.20
    public void HullAndMachinery_AllVesselTypes_ApplyCorrectFactor(VesselType vesselType, double expectedDouble)
    {
        var expected = (decimal)expectedDouble;
        var result   = _sut.CalculatePremium(500_000m, CoverageType.HullAndMachinery, vesselType);
        result.Should().Be(expected);
    }

    // ── Rounding ──────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_ResultIsRoundedToTwoDecimalPlaces()
    {
        // 100_001 * 0.0025 * 1.3 = 325.00325 -> rounds to 325.00
        var result = _sut.CalculatePremium(100_001m, CoverageType.ProtectionAndIndemnity, VesselType.Tanker);
        result.Should().Be(Math.Round(100_001m * 0.0025m * 1.3m, 2));
    }

    // ── Edge cases ────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_ZeroInsuredValue_ThrowsArgumentException()
    {
        var act = () => _sut.CalculatePremium(0m, CoverageType.Cargo, VesselType.BulkCarrier);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("insuredValue");
    }

    [Fact]
    public void Calculate_NegativeInsuredValue_ThrowsArgumentException()
    {
        var act = () => _sut.CalculatePremium(-1m, CoverageType.Cargo, VesselType.Tanker);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("insuredValue");
    }
}
