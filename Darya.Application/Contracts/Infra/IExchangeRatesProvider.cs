using System.Threading.Tasks;
using Darya.Application.Models;

namespace Darya.Application.Contracts.Infra
{
    public interface IExchangeRatesProvider
    {
        Task<ExchangeRatesResponse?> GetLatestRatesAsync(string baseCurrency, string[] targetCurrencies);
        Task<ExchangeRatesResponse?> GetHistoricalRatesAsync(string date, string baseCurrency, string[] targetCurrencies);
    }
}