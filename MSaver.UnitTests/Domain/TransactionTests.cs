namespace MSaver.UnitTests.Domain;

public sealed class TransactionTests
{
    [Fact]
    public void Create_ShouldStoreUtcDate_WhenDateKindIsUnspecified()
    {
        var date = new DateTime(2026, 6, 20, 14, 30, 0, DateTimeKind.Unspecified);

        var transaction = Transaction.Create(
            TransactionTestData.UserId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            -10m,
            date);

        transaction.Date.Kind.Should().Be(DateTimeKind.Utc);
        transaction.Date.Should().Be(DateTime.SpecifyKind(date, DateTimeKind.Utc));
    }

    [Fact]
    public void Update_ShouldStoreUtcDate_WhenDateKindIsUnspecified()
    {
        var transaction = TransactionTestData.CreateTransaction(
            TransactionTestData.UserId,
            Guid.NewGuid(),
            Guid.NewGuid());
        var date = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Unspecified);

        transaction.Update(Guid.NewGuid(), -20m, date);

        transaction.Date.Kind.Should().Be(DateTimeKind.Utc);
        transaction.Date.Should().Be(DateTime.SpecifyKind(date, DateTimeKind.Utc));
    }
}
