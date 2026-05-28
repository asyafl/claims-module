using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Persistence.Configurations;

/// <summary>
/// Demo seed data — 6 claims with all lifecycle statuses for UI demonstration.
/// </summary>
internal static class ClaimSeedData
{
    private static readonly Guid OrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    // Policy GUIDs from PolicyConfiguration
    private static readonly Guid PolMeridian    = Guid.Parse("22000000-0000-0000-0000-000000000001");
    private static readonly Guid PolHarborview  = Guid.Parse("22000000-0000-0000-0000-000000000002");
    private static readonly Guid PolCoastal     = Guid.Parse("22000000-0000-0000-0000-000000000003");
    private static readonly Guid PolStanton     = Guid.Parse("22000000-0000-0000-0000-000000000004");

    // Claim GUIDs (prefix 33...)
    public static readonly Guid Claim1Id = Guid.Parse("33000000-0000-0000-0000-000000000001");
    public static readonly Guid Claim2Id = Guid.Parse("33000000-0000-0000-0000-000000000002");
    public static readonly Guid Claim3Id = Guid.Parse("33000000-0000-0000-0000-000000000003");
    public static readonly Guid Claim4Id = Guid.Parse("33000000-0000-0000-0000-000000000004");
    public static readonly Guid Claim5Id = Guid.Parse("33000000-0000-0000-0000-000000000005");
    public static readonly Guid Claim6Id = Guid.Parse("33000000-0000-0000-0000-000000000006");

    public static Claim[] Claims() =>
    [
        Claim.CreateForSeed(Claim1Id, "CLM-2025-0000001", OrgId,
            ClaimStatus.Draft,
            PolHarborview, "POL-2024-001002", "Harborview Properties Inc",
            new DateTimeOffset(2025, 1, 20, 9, 0, 0, TimeSpan.Zero)),

        Claim.CreateForSeed(Claim2Id, "CLM-2025-0000002", OrgId,
            ClaimStatus.Open,
            PolMeridian, "POL-2024-001001", "Meridian Transport LLC",
            new DateTimeOffset(2025, 2, 10, 11, 30, 0, TimeSpan.Zero)),

        Claim.CreateForSeed(Claim3Id, "CLM-2025-0000003", OrgId,
            ClaimStatus.UnderInvestigation,
            PolStanton, "POL-2025-002002", "Stanton Medical Group",
            new DateTimeOffset(2025, 3, 5, 14, 0, 0, TimeSpan.Zero)),

        Claim.CreateForSeed(Claim4Id, "CLM-2025-0000004", OrgId,
            ClaimStatus.PendingPayment,
            PolCoastal, "POL-2025-002001", "Coastal Builders Group",
            new DateTimeOffset(2024, 11, 15, 8, 0, 0, TimeSpan.Zero)),

        Claim.CreateForSeed(Claim5Id, "CLM-2025-0000005", OrgId,
            ClaimStatus.Closed,
            PolMeridian, "POL-2024-001001", "Meridian Transport LLC",
            new DateTimeOffset(2024, 8, 1, 10, 0, 0, TimeSpan.Zero),
            closedAt:      new DateTimeOffset(2025, 1, 10, 15, 0, 0, TimeSpan.Zero),
            closureReason: "All parties settled. Reserves reconciled."),

        Claim.CreateForSeed(Claim6Id, "CLM-2025-0000006", OrgId,
            ClaimStatus.Withdrawn,
            PolHarborview, "POL-2024-001002", "Harborview Properties Inc",
            new DateTimeOffset(2025, 4, 3, 16, 0, 0, TimeSpan.Zero)),
    ];

    // LossEvent GUIDs (prefix 44...)
    public static LossEvent[] LossEvents() =>
    [
        new() { Id = Guid.Parse("44000000-0000-0000-0000-000000000001"), ClaimId = Claim1Id,
            LossDate = new DateTimeOffset(2025, 1, 18, 14, 0, 0, TimeSpan.Zero),
            LossDescription = "Electrical fire originated in server room causing damage to equipment and partial structural damage to the building.",
            LossLocation = "Kyiv, Khreshchatyk 1, Building B", CauseOfLossCode = "COL-FIRE",
            EstimatedLossAmount = 45000m, ReportDate = new DateTimeOffset(2025, 1, 20, 9, 0, 0, TimeSpan.Zero),
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2025, 1, 20, 9, 0, 0, TimeSpan.Zero) },

        new() { Id = Guid.Parse("44000000-0000-0000-0000-000000000002"), ClaimId = Claim2Id,
            LossDate = new DateTimeOffset(2025, 2, 8, 10, 15, 0, TimeSpan.Zero),
            LossDescription = "Head-on collision at intersection resulted in significant front-end damage to company vehicle and minor injuries to driver.",
            LossLocation = "Lviv, Svobody Ave 12", CauseOfLossCode = "COL-VEH-COL",
            EstimatedLossAmount = 18500m, ReportDate = new DateTimeOffset(2025, 2, 10, 11, 30, 0, TimeSpan.Zero),
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2025, 2, 10, 11, 30, 0, TimeSpan.Zero) },

        new() { Id = Guid.Parse("44000000-0000-0000-0000-000000000003"), ClaimId = Claim3Id,
            LossDate = new DateTimeOffset(2025, 3, 3, 2, 0, 0, TimeSpan.Zero),
            LossDescription = "Pharmaceutical storage area broken into overnight. Multiple controlled substance cabinets forced open. Police investigation ongoing.",
            LossLocation = "Odesa, Derybasivska 5, Medical Center", CauseOfLossCode = "COL-THEFT",
            EstimatedLossAmount = 120000m, ReportDate = new DateTimeOffset(2025, 3, 5, 14, 0, 0, TimeSpan.Zero),
            PoliceReportNumber = "OD-2025-004421",
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2025, 3, 5, 14, 0, 0, TimeSpan.Zero) },

        new() { Id = Guid.Parse("44000000-0000-0000-0000-000000000004"), ClaimId = Claim4Id,
            LossDate = new DateTimeOffset(2024, 11, 12, 6, 0, 0, TimeSpan.Zero),
            LossDescription = "Heavy rainfall caused flash flooding in construction site basement. Equipment submerged, foundation work delayed three weeks.",
            LossLocation = "Dnipro, Soborna 8, Construction Site", CauseOfLossCode = "COL-FLOOD",
            EstimatedLossAmount = 87000m, ReportDate = new DateTimeOffset(2024, 11, 15, 8, 0, 0, TimeSpan.Zero),
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2024, 11, 15, 8, 0, 0, TimeSpan.Zero) },

        new() { Id = Guid.Parse("44000000-0000-0000-0000-000000000005"), ClaimId = Claim5Id,
            LossDate = new DateTimeOffset(2024, 7, 28, 13, 0, 0, TimeSpan.Zero),
            LossDescription = "Severe wind storm damaged roof of cargo warehouse causing water ingress. Cargo contents partially destroyed.",
            LossLocation = "Kharkiv, Industrial Zone, Warehouse 14", CauseOfLossCode = "COL-WIND",
            EstimatedLossAmount = 32000m, ReportDate = new DateTimeOffset(2024, 8, 1, 10, 0, 0, TimeSpan.Zero),
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2024, 8, 1, 10, 0, 0, TimeSpan.Zero) },

        new() { Id = Guid.Parse("44000000-0000-0000-0000-000000000006"), ClaimId = Claim6Id,
            LossDate = new DateTimeOffset(2025, 4, 1, 11, 0, 0, TimeSpan.Zero),
            LossDescription = "Third party slip and fall incident in office lobby. Claimant later withdrew citing pre-existing medical condition.",
            LossLocation = "Kyiv, Business Center, Entrance Hall", CauseOfLossCode = "COL-INJURY",
            EstimatedLossAmount = 15000m, ReportDate = new DateTimeOffset(2025, 4, 3, 16, 0, 0, TimeSpan.Zero),
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2025, 4, 3, 16, 0, 0, TimeSpan.Zero) },
    ];

    public static ClaimParty[] Parties() =>
    [
        // Claim 1 - Draft (немає Claimant — навмисно, щоб показати що перехід до Open блокується)
        new() { Id = Guid.Parse("55000000-0000-0000-0000-000000000001"), ClaimId = Claim1Id,
            PartyRole = PartyRole.Insured, PartyType = PartyType.Company,
            CompanyName = "Harborview Properties Inc", Email = "claims@harborview.ua",
            Phone = "+380442001122", IsActive = true,
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2025, 1, 20, 9, 0, 0, TimeSpan.Zero) },

        // Claim 2 - Open
        new() { Id = Guid.Parse("55000000-0000-0000-0000-000000000002"), ClaimId = Claim2Id,
            PartyRole = PartyRole.Claimant, PartyType = PartyType.Person,
            FirstName = "Oleksandr", LastName = "Kovalenko", Email = "o.kovalenko@meridian.ua",
            Phone = "+380501234567", IsActive = true,
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2025, 2, 10, 11, 30, 0, TimeSpan.Zero) },

        // Claim 3 - UnderInvestigation
        new() { Id = Guid.Parse("55000000-0000-0000-0000-000000000003"), ClaimId = Claim3Id,
            PartyRole = PartyRole.Claimant, PartyType = PartyType.Company,
            CompanyName = "Stanton Medical Group", Email = "legal@stanton.ua",
            Phone = "+380487654321", IsActive = true,
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2025, 3, 5, 14, 0, 0, TimeSpan.Zero) },
        new() { Id = Guid.Parse("55000000-0000-0000-0000-000000000007"), ClaimId = Claim3Id,
            PartyRole = PartyRole.Witness, PartyType = PartyType.Person,
            FirstName = "Natalia", LastName = "Sydorenko", Phone = "+380509876543", IsActive = true,
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2025, 3, 5, 14, 0, 0, TimeSpan.Zero) },

        // Claim 4 - PendingPayment
        new() { Id = Guid.Parse("55000000-0000-0000-0000-000000000004"), ClaimId = Claim4Id,
            PartyRole = PartyRole.Claimant, PartyType = PartyType.Company,
            CompanyName = "Coastal Builders Group", Email = "insurance@coastal.ua",
            Phone = "+380661112233", IsActive = true,
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2024, 11, 15, 8, 0, 0, TimeSpan.Zero) },

        // Claim 5 - Closed
        new() { Id = Guid.Parse("55000000-0000-0000-0000-000000000005"), ClaimId = Claim5Id,
            PartyRole = PartyRole.Claimant, PartyType = PartyType.Company,
            CompanyName = "Meridian Transport LLC", Email = "claims@meridian.ua",
            Phone = "+380991234567", IsActive = true,
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2024, 8, 1, 10, 0, 0, TimeSpan.Zero) },

        // Claim 6 - Withdrawn
        new() { Id = Guid.Parse("55000000-0000-0000-0000-000000000006"), ClaimId = Claim6Id,
            PartyRole = PartyRole.Claimant, PartyType = PartyType.Person,
            FirstName = "Viktor", LastName = "Bondarenko", Email = "v.bondarenko@gmail.com",
            Phone = "+380733332211", IsActive = true,
            OrganisationId = OrgId, CreatedAt = new DateTimeOffset(2025, 4, 3, 16, 0, 0, TimeSpan.Zero) },
    ];
}
