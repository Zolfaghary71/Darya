using Darya.Application.Contracts.Infra;
using Darya.Application.Exceptions;
using Darya.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Darya.Application.Features.Rates.Queries.GetLatestExchangeRates;

public class GetExchangeRatesHandler : IRequestHandler<GetExchangeRatesQuery, ExchangeRateDto>
{
    public ICacheService _cacheService { get; set; }
    private readonly ILogger<GetExchangeRatesHandler> _logger;

    public GetExchangeRatesHandler(ILogger<GetExchangeRatesHandler> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<ExchangeRateDto> Handle(GetExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetLatestExchangeRatesValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Count > 0)
        {
            _logger.LogWarning("Validation failed  Errors: {ValidationErrors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            throw new ValidationException(validationResult);
        }
        
        var cacheKey = $"ExchangeRates:BTC:{DateTime.Now:yyyy-MM-ddTHH:mm}";

        var rate = await _cacheService.GetAsync<ExchangeRatesResponse>(cacheKey);
        
        if (rate==null)
        {
            throw new NotFoundException(nameof(ExchangeRateDto), request.Currency);
        }
        
        return new ExchangeRateDto() {Price = rate.Rates, Symbol = request.Currency, LastUpdate = DateTime.Now};
    }
}