using Darya.Domain.Entities;
using Newtonsoft.Json;

namespace Darya.Application.Models;

public class CryptocurrencyListingsResponse
{
    [JsonProperty("data")]
    public List<Cryptocurrency> Data { get; set; }

    [JsonProperty("status")]
    public Status Status { get; set; }
}