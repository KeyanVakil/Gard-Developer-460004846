using GardPortal.Models;

namespace GardPortal.Services;

public class PremiumCalculator : IPremiumCalculator
{
    private static readonly Dictionary<CoverageType, decimal> BaseRates = new()
    {
        { CoverageType.ProtectionAndIndemnity, 0.0025m },
        { CoverageType.HullAndMachinery,       0.0040m },
        { CoverageType.Cargo,                  0.0015m },
        { CoverageType.LossOfHire,             0.0020m },
    };

    private static readonly Dictionary<VesselType, decimal> VesselFactors = new()
    {
        { VesselType.Tanker,         1.3m  },
        { VesselType.Offshore,       1.4m  },
        { VesselType.BulkCarrier,    1.0m  },
        { VesselType.ContainerShip,  1.1m  },
        { VesselType.RoRo,           1.05m },
        { VesselType.GeneralCargo,   0.95m },
        { VesselType.PassengerFerry, 1.2m  },
    };

    public decimal CalculatePremium(decimal insuredValue, CoverageType coverageType, VesselType vesselType)
    {
        if (insuredValue <= 0)
            throw new ArgumentException("Insured value must be positive.", nameof(insuredValue));

        var baseRate     = BaseRates[coverageType];
        var vesselFactor = VesselFactors[vesselType];

        return Math.Round(insuredValue * baseRate * vesselFactor, 2);
    }
}
