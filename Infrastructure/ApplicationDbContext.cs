using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MSaver.Infrastructure;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TagCategory> TagCategories => Set<TagCategory>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplyUtcDateTimeConverters(modelBuilder);
    }

    private static void ApplyUtcDateTimeConverters(ModelBuilder modelBuilder)
    {
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            value => UtcDateTime.Normalize(value),
            value => UtcDateTime.SpecifyUtc(value));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            value => UtcDateTime.Normalize(value),
            value => UtcDateTime.Normalize(value));

        var dateTimeProperties = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(entityType => entityType.GetProperties())
            .Where(property => property.ClrType == typeof(DateTime) ||
                               property.ClrType == typeof(DateTime?));

        foreach (var property in dateTimeProperties)
        {
            if (property.ClrType == typeof(DateTime))
            {
                property.SetValueConverter(dateTimeConverter);
                continue;
            }

            property.SetValueConverter(nullableDateTimeConverter);
        }
    }
}
