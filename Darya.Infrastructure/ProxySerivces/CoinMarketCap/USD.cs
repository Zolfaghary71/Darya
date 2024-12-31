namespace Darya.Infrastructure.ProxySerivces.CoinMarketCap;

public class USD
{
    public double Price { get; set; }
    public double Volume24h { get; set; }
    public double VolumeChange24h { get; set; }
    public double PercentChange1h { get; set; }
    public double PercentChange24h { get; set; }
    public double PercentChange7d { get; set; }
    public double PercentChange30d { get; set; }
    public double PercentChange60d { get; set; }
    public double PercentChange90d { get; set; }
    public double MarketCap { get; set; }
    public double MarketCapDominance { get; set; }
    public double? FullyDilutedMarketCap { get; set; }
    public double? Tvl { get; set; }
    public DateTime LastUpdated { get; set; }
}