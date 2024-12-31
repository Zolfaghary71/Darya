using MediatR;

namespace Darya.Application.Features.Rates.Queries.GetLatestExchangeRates;

public class GetExchangeRatesQuery:IRequest<ExchangeRateDto>
{
    public string Currency  { get; set; }
}