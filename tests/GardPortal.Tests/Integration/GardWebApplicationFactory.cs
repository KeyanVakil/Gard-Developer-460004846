using GardPortal.Data;
using GardPortal.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GardPortal.Tests.Integration;

/// <summary>
/// Replaces SQL Server with an in-memory database and seeds baseline test data.
/// Each factory instance uses a unique database name so tests are fully isolated.
/// <br/>
/// The production Program.cs calls db.Database.Migrate() which is a no-op for the
/// InMemory provider, and then SeedData.Initialize() which populates 15 vessels and
/// associated policies/claims. We rely on that existing seed data for most tests and
/// add extra records where integration tests need specific state (e.g. a vessel whose
/// only active policy we can verify).
/// </summary>
public class GardWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    /// <summary>
    /// Returns the ID of a vessel that has at least one Active policy in the seed data.
    /// Determined lazily by querying the in-memory store after the app has started.
    /// </summary>
    public int VesselWithActivePolicyId { get; private set; }

    /// <summary>
    /// Returns the ID of the active policy belonging to <see cref="VesselWithActivePolicyId"/>.
    /// </summary>
    public int ActivePolicyId { get; private set; }

    /// <summary>
    /// Returns the ID of a vessel that has NO active policies (safe to delete in tests).
    /// </summary>
    public int VesselWithoutActivePoliciesId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the real SQL Server DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<GardDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Register InMemory context with a unique database name per factory instance
            services.AddDbContext<GardDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    /// <summary>
    /// Called by tests that need pre-resolved IDs.  Idempotent.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (VesselWithActivePolicyId != 0) return;

        // Hit the app once to trigger the production SeedData.Initialize path
        var client = CreateClient();
        await client.GetAsync("/api/vessels?pageSize=1");

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GardDbContext>();

        // Find a vessel with an active policy
        var activePolicy = await db.Policies
            .Include(p => p.Vessel)
            .FirstOrDefaultAsync(p => p.Status == PolicyStatus.Active);

        if (activePolicy is not null)
        {
            VesselWithActivePolicyId = activePolicy.VesselId;
            ActivePolicyId           = activePolicy.Id;
        }

        // Find a vessel without any active policy (for safe-delete tests)
        var vesselIds = await db.Vessels.Select(v => v.Id).ToListAsync();
        var vesselIdsWithActivePolicy = await db.Policies
            .Where(p => p.Status == PolicyStatus.Active)
            .Select(p => p.VesselId)
            .Distinct()
            .ToListAsync();

        var safeToDeleteId = vesselIds.Except(vesselIdsWithActivePolicy).FirstOrDefault();

        // If none found, create one
        if (safeToDeleteId == 0)
        {
            var newVessel = new Vessel
            {
                Name         = "MV Test Delete Safe",
                ImoNumber    = "1111111",
                VesselType   = VesselType.GeneralCargo,
                FlagState    = "Panama",
                GrossTonnage = 5_000m,
                YearBuilt    = 2000,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow,
            };
            db.Vessels.Add(newVessel);
            await db.SaveChangesAsync();
            safeToDeleteId = newVessel.Id;
        }

        VesselWithoutActivePoliciesId = safeToDeleteId;
    }
}
