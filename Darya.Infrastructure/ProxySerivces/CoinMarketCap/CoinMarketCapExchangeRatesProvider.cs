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

            // Initialize HttpClient properties
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            if (!_httpClient.DefaultRequestHeaders.Contains("X-CMC_PRO_API_KEY"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", _settings.ApiKey);
            }
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task<ExchangeRatesResponse?> GetLatestRatesAsync(string baseCurrency, string[] targetCurrencies)
{
    var endpoint = "/v1/cryptocurrency/listings/latest";
    var convertParam = string.Join(",", targetCurrencies);
    var queryParams = $"?start=1&limit=5&convert={convertParam}";

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

        // Handle only the first requested currency
        var targetCurrency = targetCurrencies.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(targetCurrency))
        {
            _logger.LogError("No target currency specified.");
            throw new ArgumentException("No target currency specified.");
        }

        double? price = targetCurrency.ToUpperInvariant() switch
        {
            "USD" => baseCrypto.Quote.USD?.Price,
            "EUR" => baseCrypto.Quote.EUR?.Price,
            "BRL" => baseCrypto.Quote.BRL?.Price,
            "GBP" => baseCrypto.Quote.GBP?.Price,
            "AUD" => baseCrypto.Quote.AUD?.Price,
            _ => null
        };

        if (price == null)
        {
            _logger.LogWarning("Target currency '{TargetCurrency}' not found or has no price data.", targetCurrency);
            throw new Exception($"Target currency '{targetCurrency}' not found or has no price data.");
        }

        var exchangeRatesResponse = new ExchangeRatesResponse
        {
            BaseCurrency = baseCurrency,
            Rates = price.Value,
            Timestamp = result.Status.Timestamp,
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
