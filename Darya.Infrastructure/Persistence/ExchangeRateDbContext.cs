using Darya.Application.Jobs;
using Darya.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Darya.Infrastructure.Persistence;

public class ExchangeRateDbContext : DbContext
{
    public DbSet<ExchangeRateEntity> ExchangeRates { get; set; }

    public ExchangeRateDbContext(DbContextOptions<ExchangeRateDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeRateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseCurrency).IsRequired().HasMaxLength(10);
            entity.Property(e => e.TargetCurrency).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Rate).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
        });
    }
}