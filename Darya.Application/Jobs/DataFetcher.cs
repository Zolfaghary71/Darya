using Darya.Application.Contracts.Infra;
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
                    var targetCurrencies = new[] { "USD", "EUR", "BRL", "GBP", "AUD" }; 

                    foreach (var targetCurrency in targetCurrencies)
                    {
                        try
                        {
                            var response = await exchangeRateProvider.GetLatestRatesAsync(baseCurrency, new[] { targetCurrency });

                            if (response != null)
                            {
                                var cacheKey = $"ExchangeRates:{baseCurrency}:{targetCurrency}:{DateTime.Now:yyyy-MM-ddTHH:mm}";
                                await cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(1));

                                _logger.LogInformation(
                                    "Exchange rate for {TargetCurrency} successfully fetched and saved at {Timestamp}.",
                                    targetCurrency,
                                    response.Timestamp
                                );
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Received null response from IExchangeRatesProvider for {TargetCurrency}.",
                                    targetCurrency
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "An error occurred while fetching and persisting exchange rate for {TargetCurrency}.",
                                targetCurrency
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the ExchangeRateBackgroundJob.");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            _logger.LogInformation("ExchangeRateBackgroundJob stopped.");
        }
    }
}
