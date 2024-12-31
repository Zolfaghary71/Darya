namespace Darya.Application.Features.Rates.Queries.GetLatestExchangeRates;

public class ExchangeRateDto
{
    public string Symbol { get; set; }
    public double Price { get; set; }
    public DateTime LastUpdate { get; set; }
}