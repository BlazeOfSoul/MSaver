using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MSaver.Infrastructure.Persistence.Configuration;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.FromCurrencyId)
            .IsRequired();

        builder.Property(x => x.ToCurrencyId)
            .IsRequired();

        builder.Property(x => x.Rate)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.Source)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FetchedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.FromCurrencyId, x.ToCurrencyId, x.FetchedAtUtc });

        builder.HasOne(x => x.FromCurrency)
            .WithMany()
            .HasForeignKey(x => x.FromCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToCurrency)
            .WithMany()
            .HasForeignKey(x => x.ToCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}