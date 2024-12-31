
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

using Darya.Application.Exceptions; 

using Darya.Application.Features.Rates.Queries.GetLatestExchangeRates;
using Darya.Application.Contracts.Infra;
using Darya.Application.Models;

using DaryaValidationException = Darya.Application.Exceptions.ValidationException;

namespace Darya.Application.Tests.Features.Rates.Queries
{
    public class GetExchangeRatesHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<ILogger<GetExchangeRatesHandler>> _loggerMock;
        private readonly GetExchangeRatesHandler _handler;

        public GetExchangeRatesHandlerTests()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<GetExchangeRatesHandler>>();

            _handler = new GetExchangeRatesHandler(
                _loggerMock.Object,
                _cacheServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_WhenCurrencyIsEmpty_ThrowsCustomValidationException()
        {

            var request = new GetExchangeRatesQuery
            {
                Currency = "" 
            };


            var exception = await Assert.ThrowsAsync<DaryaValidationException>(
                () => _handler.Handle(request, CancellationToken.None)
            );


            Assert.Contains("Currency is required.", exception.ValdationErrors);
        }

        [Fact]
        public async Task Handle_WhenCurrencyIsInvalid_ThrowsCustomValidationException()
        {
    
            var request = new GetExchangeRatesQuery
            {
                Currency = "ABC"
            };

       
           var exception = await Assert.ThrowsAsync<DaryaValidationException>(
                () => _handler.Handle(request, CancellationToken.None)
            );

         
            Assert.Contains("Currency must be one of the following: USD, EUR, GBP, JPY.",
                exception.ValdationErrors);
        }

        [Fact]
        public async Task Handle_WhenCacheIsNull_ThrowsNotFoundException()
        {
           
            var request = new GetExchangeRatesQuery
            {
                Currency = "USD" 
            };

            _cacheServiceMock
                .Setup(c => c.GetAsync<ExchangeRatesResponse>(It.IsAny<string>()))
                .ReturnsAsync((ExchangeRatesResponse)null);

            
            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(request, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_WhenEverythingIsValid_ReturnsExchangeRateDto()
        {
            var request = new GetExchangeRatesQuery
            {
                Currency = "GBP" 
            };

            var mockResponse = new ExchangeRatesResponse
            {
                Rates = 1000.50
            };

            _cacheServiceMock
                .Setup(c => c.GetAsync<ExchangeRatesResponse>(It.IsAny<string>()))
                .ReturnsAsync(mockResponse);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("GBP", result.Symbol);
            Assert.Equal(1000.50, result.Price);
            Assert.True(result.LastUpdate <= DateTime.Now,
                "LastUpdate should be set to something close to the current time");
        }
    }
}
