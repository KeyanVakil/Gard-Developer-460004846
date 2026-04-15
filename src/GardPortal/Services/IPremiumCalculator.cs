using GardPortal.Models;

namespace GardPortal.Services;

public interface IPremiumCalculator
{
    decimal CalculatePremium(decimal insuredValue, CoverageType coverageType, VesselType vesselType);
}
