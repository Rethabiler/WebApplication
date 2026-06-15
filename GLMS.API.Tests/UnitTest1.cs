using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using WebApplication.Data;
using WebApplication.Models;
using Xunit;

namespace GLMS.API.Tests
{
    // ═══════════════════════════════════════════════════════════
    // INTEGRATION TESTS FOR GLMS.API
    // These tests spin up a real in-memory version of the API
    // and test actual HTTP requests — no mocking needed.
    // ═══════════════════════════════════════════════════════════

    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _dbName = "TestDb_" + Guid.NewGuid();

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace SQL Server with InMemory database for testing
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase(_dbName));
                });
            });

            _client = _factory.CreateClient();
        }

        // ── CONTRACTS TESTS ──

        [Fact]
        public async Task GetContracts_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/ContractsApi");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetContracts_ReturnsEmptyList_WhenNoData()
        {
            var response = await _client.GetAsync("/api/ContractsApi");
            var contracts = await response.Content.ReadFromJsonAsync<List<object>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(contracts);
            Assert.Empty(contracts);
        }

        [Fact]
        public async Task GetContract_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync("/api/ContractsApi/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateContract_ReturnsBadRequest_WhenClientDoesNotExist()
        {
            var contract = new
            {
                ClientId = 9999,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(12),
                Status = "Draft",
                ServiceLevel = "Basic"
            };

            var response = await _client.PostAsJsonAsync("/api/ContractsApi", contract);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── CLIENTS TESTS ──

        [Fact]
        public async Task GetClients_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/ClientsApi");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetClients_ReturnsEmptyList_WhenNoData()
        {
            var response = await _client.GetAsync("/api/ClientsApi");
            var clients = await response.Content.ReadFromJsonAsync<List<object>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(clients);
            Assert.Empty(clients);
        }

        [Fact]
        public async Task GetClient_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync("/api/ClientsApi/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateClient_ReturnsCreated_WithValidData()
        {
            var client = new
            {
                Name = "Test Client",
                Contact = "test@client.com",
                Details = "Integration test client",
                Region = "Gauteng"
            };

            var response = await _client.PostAsJsonAsync("/api/ClientsApi", client);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task DeleteClient_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.DeleteAsync("/api/ClientsApi/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ── SERVICE REQUESTS TESTS ──

        [Fact]
        public async Task GetServiceRequests_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/ServiceRequestsApi");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetServiceRequest_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync("/api/ServiceRequestsApi/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateServiceRequest_ReturnsBadRequest_WhenContractExpired()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var client = new Client
            {
                Name = "Test",
                Contact = "test@test.com",
                Details = "test",
                Region = "Gauteng"
            };
            db.Clients.Add(client);
            await db.SaveChangesAsync();

            var contract = new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Now.AddMonths(-12),
                EndDate = DateTime.Now.AddMonths(-1),
                Status = "Expired",
                ServiceLevel = "Basic"
            };
            db.Contracts.Add(contract);
            await db.SaveChangesAsync();

            var sr = new
            {
                ContractId = contract.Id,
                Description = "Test request",
                Cost = 100.00,
                Status = "Pending"
            };

            var response = await _client.PostAsJsonAsync("/api/ServiceRequestsApi", sr);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PatchContractStatus_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var body = new { status = "Active" };
            var response = await _client.PatchAsJsonAsync("/api/ContractsApi/9999/status", body);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PatchContractStatus_ReturnsBadRequest_WhenStatusInvalid()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var client = new Client
            {
                Name = "Test2",
                Contact = "test2@test.com",
                Details = "test",
                Region = "Western Cape"
            };
            db.Clients.Add(client);
            await db.SaveChangesAsync();

            var contract = new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(12),
                Status = "Draft",
                ServiceLevel = "Standard"
            };
            db.Contracts.Add(contract);
            await db.SaveChangesAsync();

            var body = new { status = "InvalidStatus" };
            var response = await _client.PatchAsJsonAsync($"/api/ContractsApi/{contract.Id}/status", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}