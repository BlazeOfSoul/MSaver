using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MSaver.Infrastructure.Persistence.Configuration;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable(TableNames.Categories);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Color)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Name, x.Type });

        builder.HasOne(x => x.User)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TagCategories)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}