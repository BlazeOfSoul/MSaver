using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MSaver.Infrastructure.Persistence.Configuration;

public sealed class TagCategoryConfiguration : IEntityTypeConfiguration<TagCategory>
{
    public void Configure(EntityTypeBuilder<TagCategory> builder)
    {
        builder.ToTable(TableNames.TagCategories);

        builder.HasKey(x => new { x.TagId, x.CategoryId });

        builder.Property(x => x.TagId)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .IsRequired();

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.TagCategories)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.TagCategories)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}