using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MSaver.Infrastructure.Persistence.Configuration;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable(TableNames.Accounts);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Color)
            .HasMaxLength(20);

        builder.Property(x => x.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Name });

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_accounts_user_id_primary")
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true");

        builder.HasOne(x => x.User)
            .WithMany(x => x.Accounts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Account)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}