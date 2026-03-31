using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MSaver.Domain.Entities;

namespace MSaver.Infrastructure.Persistence.Configuration;

public sealed class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.ToTable("Balances");

        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Year, x.Month }).IsUnique();

        builder.Property(x => x.IncomeTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.ExpenseTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.ValueTotal).HasColumnType("numeric(18,2)");
    }
}
