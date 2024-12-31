namespace Darya.Infrastructure.ProxySerivces.CoinMarketCap;

public class ApiResponse
{
    public Status Status { get; set; }
    public List<CryptoData> Data { get; set; }
}