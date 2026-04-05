using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MSaver.Infrastructure.Persistence.Configuration;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .IsRequired();

        builder.Property(x => x.CurrencyId)
            .IsRequired();

        builder.Property(x => x.BaseCurrencyId)
            .IsRequired(false);

        builder.Property(x => x.TransferId)
            .IsRequired(false);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.AmountBase)
            .HasPrecision(18, 2);

        builder.Property(x => x.TransferRate)
            .HasPrecision(18, 8);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.TransferId);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BaseCurrency)
            .WithMany(x => x.BaseCurrencyTransactions)
            .HasForeignKey(x => x.BaseCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}