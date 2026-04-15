using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GardPortal.DTOs;
using Xunit;

namespace GardPortal.Tests.Integration;

public class DashboardApiTests : IAsyncLifetime
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

    // ── GET /api/dashboard/summary ─────────────────────────────────────────

    [Fact]
    public async Task GetSummary_ReturnsOkWithSummaryDto()
    {
        var response = await _client.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<SummaryDto>>(JsonOpts);
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSummary_SeedDataPresent_ReportsActivePoliciesAndInsuredValue()
    {
        var response = await _client.GetAsync("/api/dashboard/summary");
        var body     = await response.Content.ReadFromJsonAsync<ApiResponse<SummaryDto>>(JsonOpts);

        // Production SeedData seeds active policies; check both key aggregate fields
        body!.Data!.TotalActivePolicies.Should().BeGreaterThan(0);
        body.Data.TotalInsuredValue.Should().BeGreaterThan(0);
        body.Data.TotalAnnualPremium.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSummary_WithVesselTypeFilter_ReturnsFilteredData()
    {
        // BulkCarrier vessels are seeded; filtered summary should still return valid DTO
        var response = await _client.GetAsync("/api/dashboard/summary?vesselType=BulkCarrier");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<SummaryDto>>(JsonOpts);
        body!.Data.Should().NotBeNull();
        // Numeric fields must be non-negative regardless of filter result
        body.Data!.TotalActivePolicies.Should().BeGreaterThanOrEqualTo(0);
        body.Data.TotalInsuredValue.Should().BeGreaterThanOrEqualTo(0);
    }
}
