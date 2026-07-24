using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AssetFlow.Application.Contracts.Auth;
using AssetFlow.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace AssetFlow.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.SeedAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Register_WithValidRequest_ReturnsTokenAndTechnicianRole()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest($"user-{Guid.NewGuid():N}@assetflow.io", "Test User", "Password123");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.User.Email.Should().Be(request.Email.ToLowerInvariant());
        body.User.Role.Should().Be("Technician");
    }

    [Fact]
    public async Task Login_WithSeededAdmin_AllowsAccessToProtectedSites()
    {
        var client = _factory.CreateClient();
        var login = new LoginRequest(DbInitializer.AdminEmail, DbInitializer.AdminPassword);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", login);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var sitesResponse = await client.GetAsync("/api/sites");

        sitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var login = new LoginRequest(DbInitializer.AdminEmail, "definitely-wrong");

        var response = await client.PostAsJsonAsync("/api/auth/login", login);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSites_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/sites");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
