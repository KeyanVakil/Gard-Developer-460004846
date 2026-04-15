namespace GardPortal.Models;

public enum VesselType
{
    BulkCarrier,
    Tanker,
    ContainerShip,
    RoRo,
    GeneralCargo,
    Offshore,
    PassengerFerry
}

public enum CoverageType
{
    ProtectionAndIndemnity,
    HullAndMachinery,
    Cargo,
    LossOfHire
}

public enum PolicyStatus
{
    Draft,
    Active,
    Expired,
    Cancelled
}

public enum ClaimCategory
{
    Collision,
    Grounding,
    CargoDamage,
    CrewInjury,
    MachineryBreakdown,
    Pollution,
    Other
}

public enum ClaimStatus
{
    Reported,
    UnderReview,
    Approved,
    Settled,
    Denied
}
