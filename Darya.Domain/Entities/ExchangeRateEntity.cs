namespace Darya.Domain.Entities;

public class ExchangeRateEntity
{
    public int Id { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public string TargetCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime Timestamp { get; set; }
}