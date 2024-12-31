namespace Darya.Infrastructure.ProxySerivces.CoinMarketCap;

public class Status
{
    public DateTime Timestamp { get; set; }
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public int Elapsed { get; set; }
    public int CreditCount { get; set; }
    public string Notice { get; set; }
    public int TotalCount { get; set; }
}