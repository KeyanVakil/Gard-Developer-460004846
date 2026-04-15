using GardPortal.Models;

namespace GardPortal.Data;

public static class SeedData
{
    public static void Initialize(GardDbContext context)
    {
        if (context.Vessels.Any()) return;

        var now = DateTime.UtcNow;

        // ── Vessels (15) ──────────────────────────────────────────────────────
        var vessels = new List<Vessel>
        {
            new() { Name = "MV Nordic Star",       ImoNumber = "9123456", VesselType = VesselType.BulkCarrier,    FlagState = "Norway",       GrossTonnage = 42500, YearBuilt = 2015, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MV Atlantic Pioneer",  ImoNumber = "9234567", VesselType = VesselType.ContainerShip,  FlagState = "Panama",       GrossTonnage = 85000, YearBuilt = 2018, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MT Odin",              ImoNumber = "9345678", VesselType = VesselType.Tanker,         FlagState = "Liberia",      GrossTonnage = 110000,YearBuilt = 2012, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MV Bergen Carrier",    ImoNumber = "9456789", VesselType = VesselType.RoRo,           FlagState = "Norway",       GrossTonnage = 22000, YearBuilt = 2019, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MV Fjord Explorer",    ImoNumber = "9567890", VesselType = VesselType.PassengerFerry, FlagState = "Norway",       GrossTonnage = 18500, YearBuilt = 2020, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MT Thor Viking",       ImoNumber = "9678901", VesselType = VesselType.Tanker,         FlagState = "Marshall Islands", GrossTonnage = 95000, YearBuilt = 2014, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MV Pacific Bridge",    ImoNumber = "9789012", VesselType = VesselType.ContainerShip,  FlagState = "Singapore",    GrossTonnage = 130000,YearBuilt = 2017, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MV Stavanger Bulk",    ImoNumber = "9890123", VesselType = VesselType.BulkCarrier,    FlagState = "Norway",       GrossTonnage = 55000, YearBuilt = 2016, CreatedAt = now, UpdatedAt = now },
            new() { Name = "OSV Poseidon",         ImoNumber = "9901234", VesselType = VesselType.Offshore,       FlagState = "Norway",       GrossTonnage = 8500,  YearBuilt = 2021, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MV General Oslo",      ImoNumber = "9012345", VesselType = VesselType.GeneralCargo,   FlagState = "Bahamas",      GrossTonnage = 15000, YearBuilt = 2013, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MT Freya",             ImoNumber = "9112233", VesselType = VesselType.Tanker,         FlagState = "Malta",        GrossTonnage = 78000, YearBuilt = 2019, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MV Arctic Trader",     ImoNumber = "9223344", VesselType = VesselType.BulkCarrier,    FlagState = "Norway",       GrossTonnage = 38000, YearBuilt = 2011, CreatedAt = now, UpdatedAt = now },
            new() { Name = "OSV North Sea Hawk",   ImoNumber = "9334455", VesselType = VesselType.Offshore,       FlagState = "Norway",       GrossTonnage = 6200,  YearBuilt = 2022, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MV Baltic Express",    ImoNumber = "9445566", VesselType = VesselType.RoRo,           FlagState = "Sweden",       GrossTonnage = 19800, YearBuilt = 2018, CreatedAt = now, UpdatedAt = now },
            new() { Name = "MV Horizon Trader",    ImoNumber = "9556677", VesselType = VesselType.GeneralCargo,   FlagState = "Cyprus",       GrossTonnage = 12000, YearBuilt = 2010, CreatedAt = now, UpdatedAt = now },
        };

        context.Vessels.AddRange(vessels);
        context.SaveChanges();

        // ── Policies (26) ─────────────────────────────────────────────────────
        var policies = new List<Policy>
        {
            // Active policies
            new() { PolicyNumber = "POL-2025-00001", VesselId = vessels[0].Id, CoverageType = CoverageType.ProtectionAndIndemnity, Status = PolicyStatus.Active,   StartDate = new DateTime(2025,1,1),  EndDate = new DateTime(2026,12,31), InsuredValue = 40_000_000, AnnualPremium = 100_000, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00002", VesselId = vessels[0].Id, CoverageType = CoverageType.HullAndMachinery,        Status = PolicyStatus.Active,   StartDate = new DateTime(2025,1,1),  EndDate = new DateTime(2026,12,31), InsuredValue = 40_000_000, AnnualPremium = 160_000, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00003", VesselId = vessels[1].Id, CoverageType = CoverageType.ProtectionAndIndemnity, Status = PolicyStatus.Active,   StartDate = new DateTime(2025,3,1),  EndDate = new DateTime(2026,2,28),  InsuredValue = 80_000_000, AnnualPremium = 220_000, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00004", VesselId = vessels[2].Id, CoverageType = CoverageType.HullAndMachinery,        Status = PolicyStatus.Active,   StartDate = new DateTime(2025,2,1),  EndDate = new DateTime(2027,1,31),  InsuredValue = 95_000_000, AnnualPremium = 494_000, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00005", VesselId = vessels[3].Id, CoverageType = CoverageType.Cargo,                   Status = PolicyStatus.Active,   StartDate = new DateTime(2025,4,1),  EndDate = new DateTime(2026,3,31),  InsuredValue = 15_000_000, AnnualPremium = 23_625,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00006", VesselId = vessels[4].Id, CoverageType = CoverageType.ProtectionAndIndemnity, Status = PolicyStatus.Active,   StartDate = new DateTime(2025,5,1),  EndDate = new DateTime(2026,4,30),  InsuredValue = 25_000_000, AnnualPremium = 75_000,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00007", VesselId = vessels[5].Id, CoverageType = CoverageType.LossOfHire,              Status = PolicyStatus.Active,   StartDate = new DateTime(2025,1,15), EndDate = new DateTime(2027,1,14),  InsuredValue = 5_000_000,  AnnualPremium = 13_000,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00008", VesselId = vessels[6].Id, CoverageType = CoverageType.HullAndMachinery,        Status = PolicyStatus.Active,   StartDate = new DateTime(2025,6,1),  EndDate = new DateTime(2026,5,31),  InsuredValue = 120_000_000,AnnualPremium = 528_000, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00009", VesselId = vessels[7].Id, CoverageType = CoverageType.ProtectionAndIndemnity, Status = PolicyStatus.Active,   StartDate = new DateTime(2025,1,1),  EndDate = new DateTime(2026,12,31), InsuredValue = 50_000_000, AnnualPremium = 125_000, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00010", VesselId = vessels[8].Id, CoverageType = CoverageType.HullAndMachinery,        Status = PolicyStatus.Active,   StartDate = new DateTime(2025,7,1),  EndDate = new DateTime(2026,6,30),  InsuredValue = 30_000_000, AnnualPremium = 168_000, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00011", VesselId = vessels[9].Id, CoverageType = CoverageType.Cargo,                   Status = PolicyStatus.Active,   StartDate = new DateTime(2025,3,1),  EndDate = new DateTime(2026,2,28),  InsuredValue = 8_000_000,  AnnualPremium = 11_400,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00012", VesselId = vessels[10].Id,CoverageType = CoverageType.HullAndMachinery,        Status = PolicyStatus.Active,   StartDate = new DateTime(2025,2,1),  EndDate = new DateTime(2026,1,31),  InsuredValue = 70_000_000, AnnualPremium = 364_000, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00013", VesselId = vessels[11].Id,CoverageType = CoverageType.ProtectionAndIndemnity, Status = PolicyStatus.Active,   StartDate = new DateTime(2025,1,1),  EndDate = new DateTime(2026,12,31), InsuredValue = 35_000_000, AnnualPremium = 87_500,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00014", VesselId = vessels[12].Id,CoverageType = CoverageType.LossOfHire,              Status = PolicyStatus.Active,   StartDate = new DateTime(2025,8,1),  EndDate = new DateTime(2026,7,31),  InsuredValue = 4_000_000,  AnnualPremium = 11_200,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00015", VesselId = vessels[13].Id,CoverageType = CoverageType.Cargo,                   Status = PolicyStatus.Active,   StartDate = new DateTime(2025,4,1),  EndDate = new DateTime(2026,3,31),  InsuredValue = 12_000_000, AnnualPremium = 18_900,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00016", VesselId = vessels[14].Id,CoverageType = CoverageType.HullAndMachinery,        Status = PolicyStatus.Active,   StartDate = new DateTime(2025,6,1),  EndDate = new DateTime(2026,5,31),  InsuredValue = 9_000_000,  AnnualPremium = 34_200,  CreatedAt = now, UpdatedAt = now },
            // Expired
            new() { PolicyNumber = "POL-2024-00001", VesselId = vessels[0].Id, CoverageType = CoverageType.Cargo,                   Status = PolicyStatus.Expired,  StartDate = new DateTime(2024,1,1),  EndDate = new DateTime(2024,12,31), InsuredValue = 5_000_000,  AnnualPremium = 7_500,   CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2024-00002", VesselId = vessels[2].Id, CoverageType = CoverageType.ProtectionAndIndemnity, Status = PolicyStatus.Expired,  StartDate = new DateTime(2024,1,1),  EndDate = new DateTime(2024,12,31), InsuredValue = 90_000_000, AnnualPremium = 292_500, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2024-00003", VesselId = vessels[5].Id, CoverageType = CoverageType.Cargo,                   Status = PolicyStatus.Expired,  StartDate = new DateTime(2024,3,1),  EndDate = new DateTime(2025,2,28),  InsuredValue = 3_000_000,  AnnualPremium = 5_070,   CreatedAt = now, UpdatedAt = now },
            // Cancelled
            new() { PolicyNumber = "POL-2025-00017", VesselId = vessels[9].Id, CoverageType = CoverageType.LossOfHire,              Status = PolicyStatus.Cancelled,StartDate = new DateTime(2025,1,1),  EndDate = new DateTime(2025,12,31), InsuredValue = 2_000_000,  AnnualPremium = 3_800,   CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2025-00018", VesselId = vessels[14].Id,CoverageType = CoverageType.ProtectionAndIndemnity, Status = PolicyStatus.Cancelled,StartDate = new DateTime(2025,2,1),  EndDate = new DateTime(2026,1,31),  InsuredValue = 10_000_000, AnnualPremium = 23_750,  CreatedAt = now, UpdatedAt = now },
            // Draft
            new() { PolicyNumber = "POL-2026-00001", VesselId = vessels[1].Id, CoverageType = CoverageType.LossOfHire,              Status = PolicyStatus.Draft,    StartDate = new DateTime(2026,5,1),  EndDate = new DateTime(2027,4,30),  InsuredValue = 6_000_000,  AnnualPremium = 15_840,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2026-00002", VesselId = vessels[3].Id, CoverageType = CoverageType.HullAndMachinery,        Status = PolicyStatus.Draft,    StartDate = new DateTime(2026,6,1),  EndDate = new DateTime(2027,5,31),  InsuredValue = 20_000_000, AnnualPremium = 84_000,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2026-00003", VesselId = vessels[6].Id, CoverageType = CoverageType.Cargo,                   Status = PolicyStatus.Draft,    StartDate = new DateTime(2026,7,1),  EndDate = new DateTime(2027,6,30),  InsuredValue = 50_000_000, AnnualPremium = 82_500,  CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2026-00004", VesselId = vessels[8].Id, CoverageType = CoverageType.ProtectionAndIndemnity, Status = PolicyStatus.Draft,    StartDate = new DateTime(2026,8,1),  EndDate = new DateTime(2027,7,31),  InsuredValue = 28_000_000, AnnualPremium = 156_800, CreatedAt = now, UpdatedAt = now },
            new() { PolicyNumber = "POL-2026-00005", VesselId = vessels[11].Id,CoverageType = CoverageType.HullAndMachinery,        Status = PolicyStatus.Draft,    StartDate = new DateTime(2026,5,1),  EndDate = new DateTime(2027,4,30),  InsuredValue = 32_000_000, AnnualPremium = 128_000, CreatedAt = now, UpdatedAt = now },
        };

        context.Policies.AddRange(policies);
        context.SaveChanges();

        // ── Claims (42) ───────────────────────────────────────────────────────
        // Helper: active policy IDs for claim linking
        var activePolicies = policies.Where(p => p.Status == PolicyStatus.Active).ToList();

        var claims = new List<Claim>
        {
            // Settled claims (historical)
            new() { ClaimNumber="CLM-2025-00001", PolicyId=activePolicies[0].Id, Category=ClaimCategory.Collision,          Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2025,1,20), Description="Vessel made contact with dock fender during heavy weather berthing in Stavanger.",    EstimatedAmount=180_000, SettledAmount=155_000, CreatedAt=new DateTime(2025,1,22,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00002", PolicyId=activePolicies[2].Id, Category=ClaimCategory.CargoDamage,        Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2025,2,5),  Description="Container door seal failure caused water ingress. 12 TEU of electronics damaged.",     EstimatedAmount=420_000, SettledAmount=398_000, CreatedAt=new DateTime(2025,2,7,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00003", PolicyId=activePolicies[3].Id, Category=ClaimCategory.MachineryBreakdown, Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2025,2,18), Description="Main engine turbocharger failure requiring emergency port call and 4-day repair.",       EstimatedAmount=320_000, SettledAmount=310_000, CreatedAt=new DateTime(2025,2,20,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00004", PolicyId=activePolicies[1].Id, Category=ClaimCategory.CrewInjury,         Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2025,3,3),  Description="Crew member slipped on wet deck and sustained fractured wrist. Medical repatriation required.", EstimatedAmount=45_000, SettledAmount=42_500, CreatedAt=new DateTime(2025,3,5,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00005", PolicyId=activePolicies[4].Id, Category=ClaimCategory.CargoDamage,        Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2025,3,15), Description="RoRo cargo unit fire suppression system activation caused moisture damage to vehicles.", EstimatedAmount=250_000, SettledAmount=235_000, CreatedAt=new DateTime(2025,3,18,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00006", PolicyId=activePolicies[5].Id, Category=ClaimCategory.Grounding,          Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2025,4,2),  Description="Ferry ran aground on uncharted shoal entering Hardangerfjord. Hull damage along keel.",  EstimatedAmount=890_000, SettledAmount=875_000, CreatedAt=new DateTime(2025,4,4,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00007", PolicyId=activePolicies[6].Id, Category=ClaimCategory.Other,              Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2025,4,20), Description="Theft of deck equipment (mooring lines and winches) while berthed in Lagos.",           EstimatedAmount=28_000,  SettledAmount=26_000,  CreatedAt=new DateTime(2025,4,22,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00008", PolicyId=activePolicies[7].Id, Category=ClaimCategory.Collision,          Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2025,5,8),  Description="Vessel collided with anchored bulk carrier in Singapore anchorage during reduced visibility.",EstimatedAmount=1_200_000,SettledAmount=1_150_000,CreatedAt=new DateTime(2025,5,10,0,0,0,DateTimeKind.Utc),UpdatedAt=now },

            // Approved
            new() { ClaimNumber="CLM-2025-00009", PolicyId=activePolicies[8].Id, Category=ClaimCategory.Pollution,          Status=ClaimStatus.Approved,   IncidentDate=new DateTime(2025,6,10), Description="Minor fuel oil spill during bunkering operations in Trondheim port. 50L released.",     EstimatedAmount=85_000,  SettledAmount=null,    CreatedAt=new DateTime(2025,6,12,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00010", PolicyId=activePolicies[9].Id, Category=ClaimCategory.MachineryBreakdown, Status=ClaimStatus.Approved,   IncidentDate=new DateTime(2025,6,25), Description="Hydraulic system failure on crane #2. Boom collapse while loading in Rotterdam.",        EstimatedAmount=450_000, SettledAmount=null,    CreatedAt=new DateTime(2025,6,27,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00011", PolicyId=activePolicies[0].Id, Category=ClaimCategory.CrewInjury,         Status=ClaimStatus.Approved,   IncidentDate=new DateTime(2025,7,14), Description="Engine room crew member suffered burns during routine maintenance. 6 weeks off work.",   EstimatedAmount=62_000,  SettledAmount=null,    CreatedAt=new DateTime(2025,7,16,0,0,0,DateTimeKind.Utc), UpdatedAt=now },

            // Under Review
            new() { ClaimNumber="CLM-2025-00012", PolicyId=activePolicies[10].Id,Category=ClaimCategory.CargoDamage,        Status=ClaimStatus.UnderReview,IncidentDate=new DateTime(2025,8,3),  Description="Bulk cargo contamination discovered on arrival Rotterdam. 1,200 MT rejected.",          EstimatedAmount=390_000, SettledAmount=null,    CreatedAt=new DateTime(2025,8,5,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00013", PolicyId=activePolicies[11].Id,Category=ClaimCategory.Grounding,          Status=ClaimStatus.UnderReview,IncidentDate=new DateTime(2025,8,20), Description="Tanker grounded while navigating strait of Bonifacio in strong currents. Propeller damage.", EstimatedAmount=780_000, SettledAmount=null,    CreatedAt=new DateTime(2025,8,22,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00014", PolicyId=activePolicies[12].Id,Category=ClaimCategory.Collision,          Status=ClaimStatus.UnderReview,IncidentDate=new DateTime(2025,9,5),  Description="Vessel struck by swinging crane jib in Bremerhaven port. Starboard bridge damage.",      EstimatedAmount=220_000, SettledAmount=null,    CreatedAt=new DateTime(2025,9,7,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00015", PolicyId=activePolicies[13].Id,Category=ClaimCategory.MachineryBreakdown, Status=ClaimStatus.UnderReview,IncidentDate=new DateTime(2025,9,18), Description="Complete propulsion loss in North Sea. Emergency tow required to Aberdeen.",               EstimatedAmount=650_000, SettledAmount=null,    CreatedAt=new DateTime(2025,9,20,0,0,0,DateTimeKind.Utc), UpdatedAt=now },

            // Denied
            new() { ClaimNumber="CLM-2025-00016", PolicyId=activePolicies[1].Id, Category=ClaimCategory.Pollution,          Status=ClaimStatus.Denied,     IncidentDate=new DateTime(2025,7,30), Description="Alleged pollution from vessel in Nigerian waters. Investigation found unrelated source.", EstimatedAmount=150_000, SettledAmount=null,    CreatedAt=new DateTime(2025,8,1,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00017", PolicyId=activePolicies[3].Id, Category=ClaimCategory.CargoDamage,        Status=ClaimStatus.Denied,     IncidentDate=new DateTime(2025,8,15), Description="Cargo damage claim found to result from inadequate packing — shipper's responsibility.",  EstimatedAmount=95_000,  SettledAmount=null,    CreatedAt=new DateTime(2025,8,17,0,0,0,DateTimeKind.Utc), UpdatedAt=now },

            // Reported (recent)
            new() { ClaimNumber="CLM-2025-00018", PolicyId=activePolicies[2].Id, Category=ClaimCategory.Collision,          Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2025,10,2), Description="Vessel made contact with pier structure during docking in Hamburg. Bow thruster casing damaged.", EstimatedAmount=185_000, SettledAmount=null, CreatedAt=new DateTime(2025,10,4,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00019", PolicyId=activePolicies[4].Id, Category=ClaimCategory.CrewInjury,         Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2025,10,10),Description="Crew member fell from gangway during crew change. Leg fracture. Medivac to Bergen.",       EstimatedAmount=78_000,  SettledAmount=null,    CreatedAt=new DateTime(2025,10,12,0,0,0,DateTimeKind.Utc),UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00020", PolicyId=activePolicies[5].Id, Category=ClaimCategory.MachineryBreakdown, Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2025,10,22),Description="Steering gear hydraulic pump failure. Vessel lost steering in Dover Strait. No collision.", EstimatedAmount=230_000, SettledAmount=null,    CreatedAt=new DateTime(2025,10,24,0,0,0,DateTimeKind.Utc),UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00021", PolicyId=activePolicies[6].Id, Category=ClaimCategory.Pollution,          Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2025,11,3), Description="Bilge water discharge detected by coastal authority during routine port inspection.",       EstimatedAmount=120_000, SettledAmount=null,    CreatedAt=new DateTime(2025,11,5,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00022", PolicyId=activePolicies[7].Id, Category=ClaimCategory.CargoDamage,        Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2025,11,15),Description="Reefer unit failure led to temperature excursion. 3 TEU of pharmaceutical cargo lost.",   EstimatedAmount=550_000, SettledAmount=null,    CreatedAt=new DateTime(2025,11,17,0,0,0,DateTimeKind.Utc),UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00023", PolicyId=activePolicies[8].Id, Category=ClaimCategory.Grounding,          Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2025,11,28),Description="Vessel grounded on sandbank during low tide approach to Rotterdam Europoort.",             EstimatedAmount=340_000, SettledAmount=null,    CreatedAt=new DateTime(2025,11,30,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00024", PolicyId=activePolicies[9].Id, Category=ClaimCategory.Other,              Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2025,12,5), Description="Fire in galley due to grease accumulation. Contained by crew. Minor structural damage.",   EstimatedAmount=95_000,  SettledAmount=null,    CreatedAt=new DateTime(2025,12,7,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2025-00025", PolicyId=activePolicies[10].Id,Category=ClaimCategory.Collision,          Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2025,12,18),Description="Anchor dragged in storm, vessel allided with moored vessel. Mutual damage assessment pending.", EstimatedAmount=480_000,SettledAmount=null,CreatedAt=new DateTime(2025,12,20,0,0,0,DateTimeKind.Utc),UpdatedAt=now },

            // 2026 claims for trend data
            new() { ClaimNumber="CLM-2026-00001", PolicyId=activePolicies[11].Id,Category=ClaimCategory.MachineryBreakdown, Status=ClaimStatus.UnderReview,IncidentDate=new DateTime(2026,1,8),  Description="Exhaust gas scrubber system failure requiring urgent repair in Gothenburg.",               EstimatedAmount=280_000, SettledAmount=null,    CreatedAt=new DateTime(2026,1,10,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00002", PolicyId=activePolicies[12].Id,Category=ClaimCategory.CargoDamage,        Status=ClaimStatus.UnderReview,IncidentDate=new DateTime(2026,1,20), Description="Water ingress in hold 3 during storm. Grain cargo partial loss, 800 MT affected.",        EstimatedAmount=310_000, SettledAmount=null,    CreatedAt=new DateTime(2026,1,22,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00003", PolicyId=activePolicies[13].Id,Category=ClaimCategory.Collision,          Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2026,2,3),  Description="Vessel struck submerged debris damaging bow thruster unit. Repair in Oslo required.",     EstimatedAmount=195_000, SettledAmount=null,    CreatedAt=new DateTime(2026,2,5,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00004", PolicyId=activePolicies[14].Id,Category=ClaimCategory.CrewInjury,         Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2026,2,14), Description="Deck officer injury during cargo operations. Crane wire parted, struck officer's arm.",   EstimatedAmount=55_000,  SettledAmount=null,    CreatedAt=new DateTime(2026,2,16,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00005", PolicyId=activePolicies[0].Id, Category=ClaimCategory.Pollution,          Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2026,2,25), Description="IFO380 fuel spill during ship-to-ship transfer. 120L released. Cleanup ordered by port.",  EstimatedAmount=240_000, SettledAmount=null,    CreatedAt=new DateTime(2026,2,27,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00006", PolicyId=activePolicies[1].Id, Category=ClaimCategory.Grounding,          Status=ClaimStatus.UnderReview,IncidentDate=new DateTime(2026,3,5),  Description="Container ship grounded in Suez Canal approaches. Salvage assistance required.",           EstimatedAmount=2_100_000,SettledAmount=null,   CreatedAt=new DateTime(2026,3,7,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00007", PolicyId=activePolicies[2].Id, Category=ClaimCategory.MachineryBreakdown, Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2026,3,18), Description="Main generator failure. Vessel adrift for 14 hours before repair. No casualties.",         EstimatedAmount=175_000, SettledAmount=null,    CreatedAt=new DateTime(2026,3,20,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00008", PolicyId=activePolicies[3].Id, Category=ClaimCategory.CargoDamage,        Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2026,3,28), Description="Crude oil contamination of segregated ballast. Significant decontamination costs.",       EstimatedAmount=430_000, SettledAmount=null,    CreatedAt=new DateTime(2026,3,30,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00009", PolicyId=activePolicies[4].Id, Category=ClaimCategory.Collision,          Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2026,4,5),  Description="RoRo vessel struck navigation buoy in fog. Minor hull damage to starboard bow.",          EstimatedAmount=75_000,  SettledAmount=null,    CreatedAt=new DateTime(2026,4,7,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00010", PolicyId=activePolicies[5].Id, Category=ClaimCategory.Other,              Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2026,4,10), Description="Piracy boarding incident off West Africa. Crew unharmed, minor equipment theft.",          EstimatedAmount=38_000,  SettledAmount=null,    CreatedAt=new DateTime(2026,4,12,0,0,0,DateTimeKind.Utc), UpdatedAt=now },
            new() { ClaimNumber="CLM-2026-00011", PolicyId=activePolicies[6].Id, Category=ClaimCategory.CrewInjury,         Status=ClaimStatus.Reported,   IncidentDate=new DateTime(2026,4,13), Description="Stoker suffered heat stroke in engine room during tropical passage. Hospitalised.",        EstimatedAmount=32_000,  SettledAmount=null,    CreatedAt=new DateTime(2026,4,14,0,0,0,DateTimeKind.Utc), UpdatedAt=now },

            // Additional claims to hit 42+
            new() { ClaimNumber="CLM-2024-00001", PolicyId=activePolicies[7].Id, Category=ClaimCategory.Collision,          Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2024,9,5),  Description="Vessel collided with fishing boat in North Sea. Fishing vessel total loss.",              EstimatedAmount=850_000, SettledAmount=820_000, CreatedAt=new DateTime(2024,9,7,0,0,0,DateTimeKind.Utc),  UpdatedAt=now },
            new() { ClaimNumber="CLM-2024-00002", PolicyId=activePolicies[8].Id, Category=ClaimCategory.CargoDamage,        Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2024,10,12),Description="Steel coils shifted during storm. Securing failed. 400 MT written off.",                EstimatedAmount=560_000, SettledAmount=530_000, CreatedAt=new DateTime(2024,10,14,0,0,0,DateTimeKind.Utc),UpdatedAt=now },
            new() { ClaimNumber="CLM-2024-00003", PolicyId=activePolicies[9].Id, Category=ClaimCategory.Grounding,          Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2024,11,22),Description="Offshore vessel grounded in fog near Stavanger. Dry-dock repair needed.",                EstimatedAmount=1_400_000,SettledAmount=1_360_000,CreatedAt=new DateTime(2024,11,24,0,0,0,DateTimeKind.Utc),UpdatedAt=now },
            new() { ClaimNumber="CLM-2024-00004", PolicyId=activePolicies[10].Id,Category=ClaimCategory.MachineryBreakdown, Status=ClaimStatus.Settled,    IncidentDate=new DateTime(2024,12,8), Description="Main engine connecting rod failure. Complete overhaul required at Hyundai shipyard.",     EstimatedAmount=920_000, SettledAmount=895_000, CreatedAt=new DateTime(2024,12,10,0,0,0,DateTimeKind.Utc),UpdatedAt=now },
        };

        context.Claims.AddRange(claims);
        context.SaveChanges();

        // ── Claim History entries ─────────────────────────────────────────────
        var histories = new List<ClaimHistory>();
        foreach (var claim in claims)
        {
            // Every claim has an initial "Reported" entry
            histories.Add(new ClaimHistory
            {
                ClaimId   = claim.Id,
                FromStatus = ClaimStatus.Reported,
                ToStatus   = ClaimStatus.Reported,
                Notes      = "Claim filed and registered in system.",
                ChangedAt  = claim.CreatedAt,
                ChangedBy  = "System"
            });

            if (claim.Status == ClaimStatus.UnderReview || claim.Status == ClaimStatus.Approved || claim.Status == ClaimStatus.Settled || claim.Status == ClaimStatus.Denied)
            {
                histories.Add(new ClaimHistory { ClaimId = claim.Id, FromStatus = ClaimStatus.Reported, ToStatus = ClaimStatus.UnderReview, Notes = "Claim assigned to adjuster for review. Supporting documentation requested.", ChangedAt = claim.CreatedAt.AddDays(2), ChangedBy = "Claims Handler" });
            }
            if (claim.Status == ClaimStatus.Approved || claim.Status == ClaimStatus.Settled)
            {
                histories.Add(new ClaimHistory { ClaimId = claim.Id, FromStatus = ClaimStatus.UnderReview, ToStatus = ClaimStatus.Approved, Notes = "Surveyor report received. Claim approved subject to policy terms.", ChangedAt = claim.CreatedAt.AddDays(10), ChangedBy = "Senior Adjuster" });
            }
            if (claim.Status == ClaimStatus.Settled)
            {
                histories.Add(new ClaimHistory { ClaimId = claim.Id, FromStatus = ClaimStatus.Approved, ToStatus = ClaimStatus.Settled, Notes = $"Settlement agreed. Payment of {claim.SettledAmount:C0} authorised.", ChangedAt = claim.CreatedAt.AddDays(20), ChangedBy = "Claims Manager" });
            }
            if (claim.Status == ClaimStatus.Denied)
            {
                histories.Add(new ClaimHistory { ClaimId = claim.Id, FromStatus = ClaimStatus.UnderReview, ToStatus = ClaimStatus.Denied, Notes = "Claim denied — liability not established under policy terms.", ChangedAt = claim.CreatedAt.AddDays(8), ChangedBy = "Senior Adjuster" });
            }
        }

        context.ClaimHistories.AddRange(histories);
        context.SaveChanges();
    }
}
