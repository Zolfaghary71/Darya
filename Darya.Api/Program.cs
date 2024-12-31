using Darya.Application.Contracts;
using Darya.Application.Contracts.Infra;
using Darya.Application.Jobs;
using Darya.Infrastructure.Persistence;
using Darya.Infrastructure.ProxySerivces;
using Darya.Infrastructure.ProxySerivces.CoinMarketCap;
using Darya.Infrastructure.ProxySerivces.ExchangeRatesApi;
using Darya.Infrastructure.ProxyServices.CoinMarketCap;
using Darya.Infrastructure.ProxyServices.ExchangeRatesApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ExchangeRateBackgroundJob>();
builder.Services.AddDbContext<ExchangeRateDbContext>(options => options.UseInMemoryDatabase("Db"));
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
// builder.Services.AddScoped<IExchangeRatesProvider, ExchangeRatesApiService>();
builder.Services.AddScoped<IExchangeRatesProvider, CoinMarketCapExchangeRatesProvider>();
builder.Services.AddScoped<ICacheService,MemoryCacheService>();
builder.Services.Configure<ExchangeRatesApiSettings>(builder.Configuration.GetSection("ExchangeRatesApiSettings"));
builder.Services.Configure<CoinMarketCapSettings>(builder.Configuration.GetSection("CoinMarketCap"));

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();