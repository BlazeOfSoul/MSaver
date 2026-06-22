namespace MSaver.UnitTests.Common.Models;

public sealed class ListQueryHelperTests
{
    [Theory]
    [InlineData(" asc ", ListQueryDefaults.SortAscending)]
    [InlineData(" DESC ", ListQueryDefaults.SortDescending)]
    public void NormalizeSortDirection_ShouldTrimWhitespace(
        string value,
        string expected)
    {
        var result = ListQueryHelper.NormalizeSortDirection(value);

        result.Should().Be(expected);
    }
}
