using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GardPortal.DTOs;
using Xunit;

namespace GardPortal.Tests.Integration;

/// <summary>
/// Each test method that mutates state (POST claim, PATCH status) needs its own
/// factory instance so in-memory state does not bleed between tests. Tests that
/// only read (GET) share the class-level factory safely.
/// </summary>
public class ClaimsApiTests : IAsyncLifetime
{
    private readonly GardWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private int _activePolicyId;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        await _factory.InitializeAsync();
        _activePolicyId = _factory.ActivePolicyId;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    // ── GET /api/claims ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResponse()
    {
        var response = await _client.GetAsync("/api/claims");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ClaimDto>>(JsonOpts);
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        // TotalCount may be 0 if no claims seeded, but response structure must be valid
        body.Page.Should().Be(1);
    }

    // ── POST /api/claims ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidClaimAgainstActivePolicy_ReturnsCreated()
    {
        var dto = new
        {
            policyId        = _activePolicyId,
            category        = "Collision",
            incidentDate    = DateTime.UtcNow.AddDays(-3),
            description     = "Collision with dock structure during berthing manoeuvre.",
            estimatedAmount = 75_000m,
        };

        var response = await _client.PostAsJsonAsync("/api/claims", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ClaimDto>>(JsonOpts);
        body!.Data.Should().NotBeNull();
        body.Data!.Status.Should().Be("Reported");
        body.Data.ClaimNumber.Should().StartWith("CLM-");
        body.Data.PolicyId.Should().Be(_activePolicyId);
    }

    [Fact]
    public async Task Create_NonExistentPolicy_ReturnsNotFound()
    {
        var dto = new
        {
            policyId        = 999999,
            category        = "Grounding",
            incidentDate    = DateTime.UtcNow.AddDays(-1),
            description     = "Grounding on uncharted reef near port approach.",
            estimatedAmount = 500_000m,
        };

        var response = await _client.PostAsJsonAsync("/api/claims", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PATCH /api/claims/{id}/status ──────────────────────────────────────

    [Fact]
    public async Task TransitionStatus_ReportedToUnderReview_ReturnsOk()
    {
        // Create a fresh claim
        var createDto = new
        {
            policyId        = _activePolicyId,
            category        = "MachineryBreakdown",
            incidentDate    = DateTime.UtcNow.AddDays(-2),
            description     = "Main engine failure during transit; vessel adrift for 6 hours.",
            estimatedAmount = 200_000m,
        };
        var createResponse = await _client.PostAsJsonAsync("/api/claims", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "claim creation must succeed before status transition test can run");

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ClaimDto>>(JsonOpts);
        var claimId = created!.Data!.Id;

        // Transition to UnderReview
        var patchDto = new { newStatus = "UnderReview", notes = "Assigning marine surveyor." };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/claims/{claimId}/status", patchDto);

        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await patchResponse.Content.ReadFromJsonAsync<ApiResponse<ClaimDetailDto>>(JsonOpts);
        body!.Data!.Status.Should().Be("UnderReview");
        body.Data.History.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task TransitionStatus_InvalidTransition_ReturnsConflict()
    {
        // Create a claim (starts at Reported)
        var createDto = new
        {
            policyId        = _activePolicyId,
            category        = "Pollution",
            incidentDate    = DateTime.UtcNow.AddDays(-4),
            description     = "Oil spill from ruptured bunker fuel tank during rough weather.",
            estimatedAmount = 300_000m,
        };
        var createResponse = await _client.PostAsJsonAsync("/api/claims", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ClaimDto>>(JsonOpts);
        var claimId = created!.Data!.Id;

        // Attempt to jump directly from Reported -> Approved (skips UnderReview — invalid)
        var patchDto = new { newStatus = "Approved" };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/claims/{claimId}/status", patchDto);

        patchResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task TransitionStatus_NonExistentClaim_ReturnsNotFound()
    {
        var patchDto = new { newStatus = "UnderReview" };
        var response = await _client.PatchAsJsonAsync("/api/claims/999999/status", patchDto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
