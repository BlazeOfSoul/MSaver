using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MSaver.Infrastructure.Persistence.Configuration;

public sealed class TransactionTagConfiguration : IEntityTypeConfiguration<TransactionTag>
{
    public void Configure(EntityTypeBuilder<TransactionTag> builder)
    {
        builder.ToTable("transaction_tags");

        builder.HasKey(x => new { x.TransactionId, x.TagId });

        builder.Property(x => x.TransactionId)
            .IsRequired();

        builder.Property(x => x.TagId)
            .IsRequired();

        builder.HasOne(x => x.Transaction)
            .WithMany(x => x.TransactionTags)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.TransactionTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}