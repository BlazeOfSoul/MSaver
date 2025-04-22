using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using server.Models;

namespace server.Data.Configuration;

public class MonthlyBalanceConfiguration : IEntityTypeConfiguration<MonthlyBalance>
{
    public void Configure(EntityTypeBuilder<MonthlyBalance> builder)
    {
        builder.ToTable("MonthlyBalances");

        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Year, x.Month }).IsUnique();

        builder.Property(x => x.IncomeTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.ExpenseTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Balance).HasColumnType("numeric(18,2)");
    }
}
