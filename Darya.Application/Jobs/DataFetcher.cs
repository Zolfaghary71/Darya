using Darya.Application.Contracts;
using Darya.Application.Contracts.Infra;
using Darya.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Darya.Application.Jobs;

public class ExchangeRateBackgroundJob : BackgroundService
{
    private readonly ILogger<ExchangeRateBackgroundJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ExchangeRateBackgroundJob(ILogger<ExchangeRateBackgroundJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExchangeRateBackgroundJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var exchangeRateService = scope.ServiceProvider.GetRequiredService<IExchangeRatesService>();
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                var repository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();

                var baseCurrency = "BTC";
                // var targetCurrencies = new[] { "USD", "EUR", "BRL", "GBP", "AUD" };
                var targetCurrencies = new[] {"EUR"};


                var response = await exchangeRateService.GetLatestRatesAsync(baseCurrency, targetCurrencies);

                if (response != null && response.Success)
                {
                    var cacheKey = $"ExchangeRates:{baseCurrency}:{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}";
                    await cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(1));

                    var exchangeRateEntities = response.Rates.Select(rate => new ExchangeRateEntity
                    {
                        BaseCurrency = baseCurrency,
                        TargetCurrency = rate.Key,
                        Rate = rate.Value,
                        Timestamp = DateTime.UtcNow
                    }).ToList();

                    await repository.SaveExchangeRatesAsync(exchangeRateEntities);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching and persisting exchange rates.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _logger.LogInformation("ExchangeRateBackgroundJob stopped.");
    }
}