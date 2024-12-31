using FluentValidation;

namespace Darya.Application.Features.Rates.Queries.GetLatestExchangeRates;

public class GetLatestExchangeRatesValidator:AbstractValidator<GetExchangeRatesQuery>
{
    public GetLatestExchangeRatesValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(BeAValidCurrency).WithMessage("Currency must be one of the following: USD, EUR, GBP, JPY.");
    }
    private bool BeAValidCurrency(string currency)
    {
        var validCurrencies = new[] { "USD", "EUR", "GBP", "JPY" };
        return validCurrencies.Contains(currency);
    }
}