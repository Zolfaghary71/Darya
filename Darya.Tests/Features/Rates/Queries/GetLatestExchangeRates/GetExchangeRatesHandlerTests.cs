using System;
using System.Threading;
using System.Threading.Tasks;
using Darya.Application.Contracts.Infra;
using Darya.Application.Exceptions;
using Darya.Application.Features.Rates.Queries.GetLatestExchangeRates;
using Darya.Application.Models;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Darya.Application.Tests.Features.Rates.Queries
{
    public class GetExchangeRatesHandlerTests
    {
        
        [Fact]
        public async Task Handle_WhenRateNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GetExchangeRatesHandler>>();
            var cacheServiceMock = new Mock<ICacheService>();

            var request = new GetExchangeRatesQuery { Currency = "USD" };
            var cacheKey = $"ExchangeRates:BTC:{request.Currency}:{DateTime.Now:yyyy-MM-ddTHH:mm}";

            cacheServiceMock
                .Setup(cs => cs.GetAsync<ExchangeRatesResponse>(cacheKey))
                .ReturnsAsync((ExchangeRatesResponse)null); // Simulate missing data

            var handler = new GetExchangeRatesHandler(loggerMock.Object, cacheServiceMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));

            Assert.Contains(nameof(ExchangeRateDto), exception.Message);
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenValidRequest_ReturnsExchangeRateDto()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GetExchangeRatesHandler>>();
            var cacheServiceMock = new Mock<ICacheService>();

            var request = new GetExchangeRatesQuery { Currency = "USD" };
            var cacheKey = $"ExchangeRates:BTC:{request.Currency}:{DateTime.Now:yyyy-MM-ddTHH:mm}";

            var expectedRate = new ExchangeRatesResponse
            {
                Rates = 12345.67,
                Timestamp = DateTime.Now
            };

            cacheServiceMock
                .Setup(cs => cs.GetAsync<ExchangeRatesResponse>(cacheKey))
                .ReturnsAsync(expectedRate);

            var handler = new GetExchangeRatesHandler(loggerMock.Object, cacheServiceMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Currency, result.Symbol);
            Assert.Equal(expectedRate.Rates, result.Price);
            Assert.Equal(DateTime.Now.Date, result.LastUpdate.Date);
            cacheServiceMock.Verify(
                cs => cs.GetAsync<ExchangeRatesResponse>(cacheKey),
                Times.Once);
        }
    }
}
