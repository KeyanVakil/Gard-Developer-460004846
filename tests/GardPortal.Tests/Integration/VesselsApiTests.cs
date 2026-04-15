using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GardPortal.DTOs;
using Xunit;

namespace GardPortal.Tests.Integration;

public class VesselsApiTests : IAsyncLifetime
{
    private readonly GardWebApplicationFactory _factory = new();
    private HttpClient _client = null!;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        await _factory.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    // ── GET /api/vessels ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResponse()
    {
        var response = await _client.GetAsync("/api/vessels");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<VesselDto>>(JsonOpts);
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_SeedData_ReturnsMultipleVessels()
    {
        var response = await _client.GetAsync("/api/vessels?pageSize=50");
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<VesselDto>>(JsonOpts);

        // Production SeedData seeds 15 vessels
        body!.TotalCount.Should().BeGreaterThanOrEqualTo(5);
    }

    // ── GET /api/vessels/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingVessel_ReturnsVesselDetail()
    {
        var id       = _factory.VesselWithActivePolicyId;
        var response = await _client.GetAsync($"/api/vessels/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<VesselDetailDto>>(JsonOpts);
        body!.Data.Should().NotBeNull();
        body.Data!.Id.Should().Be(id);
        body.Data.ActivePoliciesCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetById_NonExistentVessel_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/vessels/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /api/vessels ──────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidPayload_ReturnsCreatedWithVesselDto()
    {
        var dto = new
        {
            name         = "MV Integration New",
            imoNumber    = "8888881",
            vesselType   = "BulkCarrier",
            flagState    = "Panama",
            grossTonnage = 40000,
            yearBuilt    = 2022,
        };

        var response = await _client.PostAsJsonAsync("/api/vessels", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<VesselDto>>(JsonOpts);
        body!.Data.Should().NotBeNull();
        body.Data!.Name.Should().Be("MV Integration New");
        body.Data.ImoNumber.Should().Be("8888881");
        response.Headers.Location.Should().NotBeNull();
    }

    // ── DELETE /api/vessels/{id} with active policy ────────────────────────

    [Fact]
    public async Task Delete_VesselWithActivePolicy_ReturnsConflict()
    {
        var id       = _factory.VesselWithActivePolicyId;
        var response = await _client.DeleteAsync($"/api/vessels/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
