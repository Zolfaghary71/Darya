namespace Darya.Infrastructure.ProxySerivces.CoinMarketCap;

public class CryptoData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public string Slug { get; set; }
    public int NumMarketPairs { get; set; }
    public DateTime DateAdded { get; set; }
    public List<string> Tags { get; set; }
    public long? MaxSupply { get; set; }
    public double CirculatingSupply { get; set; }
    public double TotalSupply { get; set; }
    public bool InfiniteSupply { get; set; }
    public Platform Platform { get; set; }
    public int CmcRank { get; set; }
    public double? SelfReportedCirculatingSupply { get; set; }
    public double? SelfReportedMarketCap { get; set; }
    public double? TvlRatio { get; set; }
    public DateTime LastUpdated { get; set; }
    public Quote Quote { get; set; }
}