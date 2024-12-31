using Newtonsoft.Json;

namespace Darya.Domain.Entities;

public class QuoteDetail
{
    [JsonProperty("price")]
    public decimal Price { get; set; }
}