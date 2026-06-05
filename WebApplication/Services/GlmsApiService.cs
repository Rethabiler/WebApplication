using System.Text;
using System.Text.Json;
using WebApplication.Models;

namespace WebApplication.Services
{
    // This service replaces direct DB access in MVC controllers.
    // All data now comes from the GLMS.API via HttpClient.
    public interface IGlmsApiService
    {
        // Clients
        Task<List<Client>> GetClientsAsync(string? search = null);
        Task<Client?> GetClientAsync(int id);
        Task<bool> CreateClientAsync(Client client);
        Task<bool> UpdateClientAsync(int id, Client client);
        Task<bool> DeleteClientAsync(int id);

        // Contracts
        Task<List<Contract>> GetContractsAsync(string? status = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<Contract?> GetContractAsync(int id);
        Task<bool> CreateContractAsync(Contract contract);
        Task<bool> UpdateContractStatusAsync(int id, string status);
        Task<bool> DeleteContractAsync(int id);

        // Service Requests
        Task<List<ServiceRequest>> GetServiceRequestsAsync(int? contractId = null);
        Task<ServiceRequest?> GetServiceRequestAsync(int id);
        Task<bool> CreateServiceRequestAsync(ServiceRequest serviceRequest);
        Task<bool> DeleteServiceRequestAsync(int id);
    }

    public class GlmsApiService : IGlmsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GlmsApiService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GlmsApiService(HttpClient httpClient, ILogger<GlmsApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ── CLIENTS ──

        public async Task<List<Client>> GetClientsAsync(string? search = null)
        {
            var url = "/api/ClientsApi";
            if (!string.IsNullOrEmpty(search))
                url += $"?search={Uri.EscapeDataString(search)}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<Client>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Client>>(json, _jsonOptions) ?? new List<Client>();
        }

        public async Task<Client?> GetClientAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/ClientsApi/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Client>(json, _jsonOptions);
        }

        public async Task<bool> CreateClientAsync(Client client)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(client), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/ClientsApi", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateClientAsync(int id, Client client)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(client), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/ClientsApi/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/ClientsApi/{id}");
            return response.IsSuccessStatusCode;
        }

        // ── CONTRACTS ──

        public async Task<List<Contract>> GetContractsAsync(string? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = "/api/ContractsApi?";
            if (!string.IsNullOrEmpty(status)) url += $"status={status}&";
            if (startDate.HasValue) url += $"startDate={startDate.Value:yyyy-MM-dd}&";
            if (endDate.HasValue) url += $"endDate={endDate.Value:yyyy-MM-dd}&";

            var response = await _httpClient.GetAsync(url.TrimEnd('&', '?'));
            if (!response.IsSuccessStatusCode) return new List<Contract>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Contract>>(json, _jsonOptions) ?? new List<Contract>();
        }

        public async Task<Contract?> GetContractAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/ContractsApi/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Contract>(json, _jsonOptions);
        }

        public async Task<bool> CreateContractAsync(Contract contract)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(contract), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/ContractsApi", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateContractStatusAsync(int id, string status)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { status }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"/api/ContractsApi/{id}/status", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/ContractsApi/{id}");
            return response.IsSuccessStatusCode;
        }

        // ── SERVICE REQUESTS ──

        public async Task<List<ServiceRequest>> GetServiceRequestsAsync(int? contractId = null)
        {
            var url = "/api/ServiceRequestsApi";
            if (contractId.HasValue) url += $"?contractId={contractId}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<ServiceRequest>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ServiceRequest>>(json, _jsonOptions) ?? new List<ServiceRequest>();
        }

        public async Task<ServiceRequest?> GetServiceRequestAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/ServiceRequestsApi/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceRequest>(json, _jsonOptions);
        }

        public async Task<bool> CreateServiceRequestAsync(ServiceRequest serviceRequest)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(serviceRequest), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/ServiceRequestsApi", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteServiceRequestAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/ServiceRequestsApi/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}