namespace MSaver.UnitTests.Domain;

public sealed class TagTests
{
    [Fact]
    public void ReplaceCategories_ShouldKeepExistingLinks_WhenCategoryRemainsAssigned()
    {
        var tag = TagTestData.CreateTag(TagTestData.UserId);
        var existingCategoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();

        tag.ReplaceCategories([existingCategoryId]);
        var existingLink = tag.TagCategories.Single();

        tag.ReplaceCategories([existingCategoryId, newCategoryId]);

        tag.TagCategories.Should().HaveCount(2);
        tag.TagCategories.Should().Contain(x => ReferenceEquals(x, existingLink));
        tag.TagCategories.Select(x => x.CategoryId).Should().BeEquivalentTo([
            existingCategoryId,
            newCategoryId
        ]);
    }
}
