using System.Text;
using Darya.Api.Middleware;
using Darya.Application.Contracts;
using Darya.Application.Contracts.Infra;
using Darya.Application.Features.Rates.Queries;
using Darya.Application.Features.Rates.Queries.GetLatestExchangeRates;
using Darya.Application.Jobs;
using Darya.Infrastructure.Persistence;
using Darya.Infrastructure.ProxySerivces;
using Darya.Infrastructure.ProxySerivces.CoinMarketCap;
using Darya.Infrastructure.ProxySerivces.ExchangeRatesApi;
using Darya.Infrastructure.TokenService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtSettings = Darya.Api.Models.JwtSettings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ExchangeRatesApiSettings>(builder.Configuration.GetSection("ExchangeRatesApiSettings"));

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<ExchangeRateBackgroundJob>();
builder.Services.AddDbContext<ExchangeRateDbContext>(options => options.UseInMemoryDatabase("Db"));
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddScoped<IExchangeRatesProvider, CoinMarketCapExchangeRatesProvider>();
builder.Services.AddScoped<ICacheService,MemoryCacheService>();
builder.Services.Configure<ExchangeRatesApiSettings>(builder.Configuration.GetSection("ExchangeRatesApiSettings"));
builder.Services.Configure<CoinMarketCapSettings>(builder.Configuration.GetSection("CoinMarketCap"));
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetExchangeRatesHandler).Assembly));

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();