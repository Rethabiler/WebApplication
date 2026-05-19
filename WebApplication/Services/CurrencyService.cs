using System.Text.Json;

namespace WebApplication.Services
{
    // The interface defines the contract — what the service can do.
    
    public interface ICurrencyService
    {
        Task<decimal> GetUsdToZarRateAsync();
        decimal ConvertUsdToZar(decimal usdAmount, decimal rate);
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;

        
        private decimal _cachedRate = 0;
        private DateTime _cacheExpiry = DateTime.MinValue;

        public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            // Return cached rate if still valid
            if (_cachedRate > 0 && DateTime.UtcNow < _cacheExpiry)
                return _cachedRate;

            try
            {
                // Free API — no key needed
                var response = await _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var zarRate = doc.RootElement
                    .GetProperty("rates")
                    .GetProperty("ZAR")
                    .GetDecimal();

                // Cache for 1 hour
                _cachedRate = zarRate;
                _cacheExpiry = DateTime.UtcNow.AddHours(1);

                _logger.LogInformation("Fetched USD-ZAR rate: {Rate}", zarRate);
                return zarRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch currency rate. Using fallback.");
                // Fallback rate if API is unreachable
                return 18.50m;
            }
        }

        // Pure math — easy to unit test
        public decimal ConvertUsdToZar(decimal usdAmount, decimal rate)
        {
            if (rate <= 0)
                throw new ArgumentException("Rate must be greater than zero.", nameof(rate));

            return Math.Round(usdAmount * rate, 2);
        }
    }
}
