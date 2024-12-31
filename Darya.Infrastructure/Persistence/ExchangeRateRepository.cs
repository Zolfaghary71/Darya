using Darya.Application.Contracts.Infra;
using Darya.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Darya.Infrastructure.Persistence;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly ExchangeRateDbContext _context;

    public ExchangeRateRepository(ExchangeRateDbContext context)
    {
        _context = context;
    }

    public async Task SaveExchangeRatesAsync(IEnumerable<ExchangeRateEntity> exchangeRates)
    {
        await _context.ExchangeRates.AddRangeAsync(exchangeRates);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ExchangeRateEntity>> GetExchangeRatesAsync(string baseCurrency, string targetCurrency, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.ExchangeRates.AsQueryable();

        if (!string.IsNullOrEmpty(baseCurrency))
            query = query.Where(e => e.BaseCurrency == baseCurrency);

        if (!string.IsNullOrEmpty(targetCurrency))
            query = query.Where(e => e.TargetCurrency == targetCurrency);

        if (startDate.HasValue)
            query = query.Where(e => e.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.Timestamp <= endDate.Value);

        return await query.ToListAsync();
    }

    public async Task<ExchangeRateEntity?> GetLatestExchangeRateAsync(string baseCurrency, string targetCurrency)
    {
        return await _context.ExchangeRates
            .Where(e => e.BaseCurrency == baseCurrency && e.TargetCurrency == targetCurrency)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteExchangeRatesOlderThanAsync(DateTime cutoffDate)
    {
        var oldRates = await _context.ExchangeRates
            .Where(e => e.Timestamp < cutoffDate)
            .ToListAsync();

        _context.ExchangeRates.RemoveRange(oldRates);
        await _context.SaveChangesAsync();
    }
}