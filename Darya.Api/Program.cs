using Darya.Application.Contracts;
using Darya.Application.Contracts.Infra;
using Darya.Application.Jobs;
using Darya.Infrastructure.Persistence;
using Darya.Infrastructure.ProxySerivces;
using Darya.Infrastructure.ProxySerivces.ExchangeRatesApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ExchangeRateBackgroundJob>();
builder.Services.AddDbContext<ExchangeRateDbContext>(options => options.UseInMemoryDatabase("Db"));
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddScoped<IExchangeRatesService, ExchangeRatesApiService>();
builder.Services.AddScoped<ICacheService,MemoryCacheService>();
builder.Services.Configure<ExchangeRatesApiSettings>(builder.Configuration.GetSection("ExchangeRatesApiSettings"));


builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();