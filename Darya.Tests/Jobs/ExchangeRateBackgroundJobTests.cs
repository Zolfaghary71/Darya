using System;
using System.Threading;
using System.Threading.Tasks;
using Darya.Application.Contracts.Infra;
using Darya.Application.Jobs;
using Darya.Application.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Darya.Application.Tests.Jobs
{
    public class ExchangeRateBackgroundJobTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenResponseIsNotNull_CachesRatesAndLogsSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExchangeRateBackgroundJob>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var exchangeRateProviderMock = new Mock<IExchangeRatesProvider>();
            var cacheServiceMock = new Mock<ICacheService>();

            var mockResponse = new ExchangeRatesResponse
            {
                Rates = 12345.67,
                Timestamp = DateTime.Now
            };

            exchangeRateProviderMock
                .Setup(p => p.GetLatestRatesAsync("BTC", It.Is<string[]>(currencies => currencies[0] == "USD")))
                .ReturnsAsync(mockResponse);

            serviceScopeFactoryMock
                .Setup(s => s.CreateScope())
                .Returns(serviceScopeMock.Object);

            serviceScopeMock
                .SetupGet(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IExchangeRatesProvider)))
                .Returns(exchangeRateProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ICacheService)))
                .Returns(cacheServiceMock.Object);

            var job = new ExchangeRateBackgroundJob(loggerMock.Object, serviceProviderMock.Object);

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

            // Act
            await job.StartAsync(cts.Token);
            await Task.Delay(300); // Allow one loop iteration

            // Assert
            exchangeRateProviderMock.Verify(
                x => x.GetLatestRatesAsync("BTC", It.Is<string[]>(currencies => currencies[0] == "USD")),
                Times.AtLeastOnce);

            cacheServiceMock.Verify(
                x => x.SetAsync(
                    It.Is<string>(key => key.Contains("ExchangeRates:BTC:USD")),
                    mockResponse,
                    It.IsAny<TimeSpan>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exchange rate for USD successfully fetched and saved")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
    }
}
