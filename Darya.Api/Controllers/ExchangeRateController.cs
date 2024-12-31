using Darya.Application.Features.Rates.Queries.GetLatestExchangeRates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Darya.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExchangeRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{currency}")]
    public async Task<IActionResult> GetLatestExchangeRates(string currency)
    {
        var query = new GetExchangeRatesQuery
        {
            Currency = currency
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
