using System.Text.Json;
using System.Web;
using Darya.Application.Contracts.Infra;
using Darya.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Darya.Infrastructure.ProxySerivces.ExchangeRatesApi
{
    public class ExchangeRatesApiService : IExchangeRatesProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ExchangeRatesApiSettings _options;
        private readonly ILogger<ExchangeRatesApiService> _logger;

        public ExchangeRatesApiService(
            HttpClient httpClient,
            IOptions<ExchangeRatesApiSettings> options,
            ILogger<ExchangeRatesApiService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            if (!string.IsNullOrEmpty(_options.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            }
        }

        public async Task<ExchangeRatesResponse?> GetLatestRatesAsync(string baseCurrency, string[] symbols)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["access_key"] = _options.ApiKey;
            query["base"] = baseCurrency;
            query["symbols"] = string.Join(",", symbols);

            var endpoint = $"latest?{query}";

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch latest rates. Status: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ExchangeRatesResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ExchangeRatesApiService.GetLatestRatesAsync");
                return null;
            }
        }

        public async Task<ExchangeRatesResponse?> GetHistoricalRatesAsync(string date, string baseCurrency, string[] symbols)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["access_key"] = _options.ApiKey;
            query["base"] = baseCurrency;
            query["symbols"] = string.Join(",", symbols);

            var endpoint = $"{date}?{query}";

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch historical rates for date {Date}. Status: {StatusCode}", date, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ExchangeRatesResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ExchangeRatesApiService.GetHistoricalRatesAsync");
                return null;
            }
        }
    }
}
