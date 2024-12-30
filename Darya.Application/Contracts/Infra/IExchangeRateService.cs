using Darya.Infrastructure.ProxySerivces.ExchangeRatesApi;

namespace Darya.Application.Contracts;

public interface IExchangeRatesService
{
    Task<ExchangeRatesResponse?> GetLatestRatesAsync(
        string baseCurrency,
        string[] symbols
    );

    Task<ExchangeRatesResponse?> GetHistoricalRatesAsync(
        string date, 
        string baseCurrency,
        string[] symbols
    );
    
}