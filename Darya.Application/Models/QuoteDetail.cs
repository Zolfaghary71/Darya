using Newtonsoft.Json;

namespace Darya.Application.Models;

public class QuoteDetail
{
    [JsonProperty("price")]
    public decimal Price { get; set; }
}