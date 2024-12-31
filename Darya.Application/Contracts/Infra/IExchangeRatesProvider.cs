using System.Threading.Tasks;
using Darya.Infrastructure.ProxySerivces.ExchangeRatesApi;

namespace Darya.Application.Contracts.Infra
{
    public interface IExchangeRatesProvider
    {
        Task<ExchangeRatesResponse?> GetLatestRatesAsync(string baseCurrency, string[] targetCurrencies);
        Task<ExchangeRatesResponse?> GetHistoricalRatesAsync(string date, string baseCurrency, string[] targetCurrencies);
    }
}