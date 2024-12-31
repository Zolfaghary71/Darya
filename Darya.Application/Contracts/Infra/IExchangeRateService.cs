using Darya.Application.Models;

namespace Darya.Application.Contracts.Infra;

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