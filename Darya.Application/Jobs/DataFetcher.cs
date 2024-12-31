using Darya.Application.Contracts;
using Darya.Application.Contracts.Infra;
using Darya.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Darya.Application.Jobs
{
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

                    var exchangeRateProvider = scope.ServiceProvider.GetRequiredService<IExchangeRatesProvider>();
                    var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                    var baseCurrency = "BTC";
                    var targetCurrencies = new[] {"USD"};
                    //the free versions of 2 APIs only suppert usd

                    var response = await exchangeRateProvider.GetLatestRatesAsync(baseCurrency, targetCurrencies);

                    if (response != null)
                    {
                        var cacheKey = $"ExchangeRates:{baseCurrency}:{DateTime.Now:yyyy-MM-ddTHH:mms}";
                        await cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(1));

                        _logger.LogInformation("Exchange rates successfully fetched and saved at {Timestamp}.", response.Timestamp);
                    }
                    else
                    {
                        _logger.LogWarning("Received null response from IExchangeRatesProvider.");
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
}
