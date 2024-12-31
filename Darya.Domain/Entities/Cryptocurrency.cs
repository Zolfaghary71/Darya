using Newtonsoft.Json;

namespace Darya.Domain.Entities;

public class Cryptocurrency
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; }

    [JsonProperty("quote")]
    public Dictionary<string, decimal> Quote { get; set; }
}