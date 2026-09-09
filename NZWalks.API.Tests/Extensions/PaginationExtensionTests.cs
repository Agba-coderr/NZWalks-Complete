using FluentAssertions;
using NZWalks.Application.Extensions;
using Xunit;

namespace NZWalks.API.Tests.Extensions
{
    public class PaginationExtensionTests
    {
        [Fact]
        public void Paginate_FirstPage_ReturnsCorrectSubset()
        {
            // Arrange
            var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.AsQueryable();

            // Act
            var result = source.Paginate(pageNumber: 1, pageSize: 3).ToList();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Equal(1, 2, 3);
        }

        [Fact]
        public void Paginate_SecondPage_ReturnsCorrectSubset()
        {
            // Arrange
            var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.AsQueryable();

            // Act
            var result = source.Paginate(pageNumber: 2, pageSize: 3).ToList();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Equal(4, 5, 6);
        }

        [Fact]
        public void Paginate_PageBeyondRange_ReturnsEmptyCollection()
        {
            // Arrange
            var source = new List<int> { 1, 2, 3 }.AsQueryable();

            // Act
            var result = source.Paginate(pageNumber: 5, pageSize: 10).ToList();

            // Assert
            result.Should().BeEmpty();
        }
    }
}
