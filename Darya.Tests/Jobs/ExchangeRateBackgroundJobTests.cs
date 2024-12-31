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

            var exchangeRatesProviderMock = new Mock<IExchangeRatesProvider>();
            var cacheServiceMock = new Mock<ICacheService>();

            var mockRatesResponse = new ExchangeRatesResponse
            {
                Rates = 9999.99,
                Timestamp = DateTime.Now
            };

            exchangeRatesProviderMock
                .Setup(p => p.GetLatestRatesAsync("BTC", It.Is<string[]>(x => x[0] == "USD")))
                .ReturnsAsync(mockRatesResponse);

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
                .Returns(exchangeRatesProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ICacheService)))
                .Returns(cacheServiceMock.Object);

            var job = new ExchangeRateBackgroundJob(loggerMock.Object, serviceProviderMock.Object);

            // Cancel after ~1 iteration to avoid an infinite loop
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

            // Act
            await job.StartAsync(cts.Token);
            await Task.Delay(300); // Give one loop iteration time

            // Assert
            exchangeRatesProviderMock.Verify(
                x => x.GetLatestRatesAsync("BTC", It.Is<string[]>(arr => arr[0] == "USD")),
                Times.AtLeastOnce
            );

            cacheServiceMock.Verify(
                x => x.SetAsync(
                    It.Is<string>(key => key.Contains("ExchangeRates:BTC")), 
                    mockRatesResponse, 
                    It.IsAny<TimeSpan>()),
                Times.Once
            );

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exchange rates successfully fetched and saved")),
                    null, // no exception in the success case
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task ExecuteAsync_WhenResponseIsNull_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExchangeRateBackgroundJob>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var exchangeRatesProviderMock = new Mock<IExchangeRatesProvider>();
            var cacheServiceMock = new Mock<ICacheService>();

            exchangeRatesProviderMock
                .Setup(p => p.GetLatestRatesAsync("BTC", It.IsAny<string[]>()))
                .ReturnsAsync((ExchangeRatesResponse)null);

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
                .Returns(exchangeRatesProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ICacheService)))
                .Returns(cacheServiceMock.Object);

            var job = new ExchangeRateBackgroundJob(loggerMock.Object, serviceProviderMock.Object);

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

            // Act
            await job.StartAsync(cts.Token);
            await Task.Delay(300);

            // Assert
            exchangeRatesProviderMock.Verify(
                x => x.GetLatestRatesAsync("BTC", It.IsAny<string[]>()),
                Times.AtLeastOnce
            );

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received null response from IExchangeRatesProvider")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.AtLeastOnce
            );

            cacheServiceMock.Verify(
                c => c.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<ExchangeRatesResponse>(),
                    It.IsAny<TimeSpan>()),
                Times.Never
            );
        }

        [Fact]
        public async Task ExecuteAsync_WhenProviderThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExchangeRateBackgroundJob>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var exchangeRatesProviderMock = new Mock<IExchangeRatesProvider>();
            var cacheServiceMock = new Mock<ICacheService>();

            // Provider throws
            exchangeRatesProviderMock
                .Setup(p => p.GetLatestRatesAsync("BTC", It.IsAny<string[]>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

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
                .Returns(exchangeRatesProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ICacheService)))
                .Returns(cacheServiceMock.Object);

            var job = new ExchangeRateBackgroundJob(loggerMock.Object, serviceProviderMock.Object);

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

            // Act
            await job.StartAsync(cts.Token);
            await Task.Delay(300);

            // Assert
            exchangeRatesProviderMock.Verify(
                x => x.GetLatestRatesAsync("BTC", It.IsAny<string[]>()),
                Times.AtLeastOnce
            );

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while fetching and persisting exchange rates")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.AtLeastOnce
            );
        }
    }
}
