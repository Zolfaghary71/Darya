using Darya.Application.Contracts.Infra;
using Darya.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Darya.Infrastructure.ProxySerivces.CoinMarketCap
{
    public class CoinMarketCapExchangeRatesProvider : IExchangeRatesProvider
    {
        private readonly HttpClient _httpClient;
        private readonly CoinMarketCapSettings _settings;
        private readonly ILogger<CoinMarketCapExchangeRatesProvider> _logger;

        public CoinMarketCapExchangeRatesProvider(
            HttpClient httpClient,
            IOptions<CoinMarketCapSettings> options,
            ILogger<CoinMarketCapExchangeRatesProvider> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<ExchangeRatesResponse?> GetLatestRatesAsync(string baseCurrency, string[] targetCurrencies)
        {
            var endpoint = "/v1/cryptocurrency/listings/latest";
            var convertParam = string.Join(",", targetCurrencies);
            var queryParams = $"?start=1&limit=5&convert={convertParam}"; 

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Remove("Accept");
            _httpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            try
            {
                _logger.LogInformation("Sending request to CoinMarketCap API: {Endpoint}{QueryParams}", endpoint, queryParams);
                var response = await _httpClient.GetAsync($"{endpoint}{queryParams}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("CoinMarketCap API responded with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"CoinMarketCap API error: {response.StatusCode}, {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (result == null)
                {
                    _logger.LogError("Failed to deserialize CoinMarketCap API response.");
                    throw new JsonException("Failed to deserialize CoinMarketCap API response.");
                }

                var baseCrypto = result.Data.FirstOrDefault(c => c.Symbol.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase));

                if (baseCrypto == null)
                {
                    _logger.LogError("Base currency '{BaseCurrency}' not found in CoinMarketCap data.", baseCurrency);
                    throw new Exception($"Base currency '{baseCurrency}' not found in CoinMarketCap data.");
                }

                var exchangeRatesResponse = new ExchangeRatesResponse
                {
                    BaseCurrency = baseCurrency,
                    Rates = result.Data.FirstOrDefault(d=>d.Symbol=="BTC").Quote.USD.Price,
                    Timestamp = result.Status.Timestamp ,
                    Success = true,
                };

                return exchangeRatesResponse;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while fetching exchange rates from CoinMarketCap.");
                throw;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while processing CoinMarketCap response.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching exchange rates from CoinMarketCap.");
                throw;
            }
        }

        public Task<ExchangeRatesResponse?> GetHistoricalRatesAsync(string date, string baseCurrency, string[] targetCurrencies)
        {
            _logger.LogWarning("Historical rates fetching is not implemented for CoinMarketCap.");
            throw new NotImplementedException("Historical rates are not implemented for CoinMarketCap.");
        }
    }

}
