using Darya.Application.Jobs;
using Darya.Domain.Entities;

namespace Darya.Application.Contracts.Infra;

public interface IExchangeRateRepository
{
    Task SaveExchangeRatesAsync(IEnumerable<ExchangeRateEntity> exchangeRates);
    Task<List<ExchangeRateEntity>> GetExchangeRatesAsync(string baseCurrency, string targetCurrency, DateTime? startDate, DateTime? endDate);
    Task<ExchangeRateEntity?> GetLatestExchangeRateAsync(string baseCurrency, string targetCurrency);
    Task DeleteExchangeRatesOlderThanAsync(DateTime cutoffDate);
}