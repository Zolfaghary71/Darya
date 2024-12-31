using Darya.Application.Models;

namespace Darya.Application.Contracts.Infra;

public interface ICoinMarketCapProxy
{
    Task<CryptocurrencyListingsResponse> GetCryptocurrencyListingsAsync(int start = 1, int limit = 5000, string convert = "USD");
}