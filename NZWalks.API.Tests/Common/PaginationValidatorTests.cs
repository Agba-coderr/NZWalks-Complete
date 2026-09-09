using FluentAssertions;
using NZWalks.Application.Common;
using Xunit;

namespace NZWalks.API.Tests.Common
{
    public class PaginationValidatorTests
    {
        [Theory]
        [InlineData(1, 10)]
        [InlineData(5, 25)]
        [InlineData(10, 50)]
        public void Validate_WhenPageNumberAndPageSizeAreValid_ReturnsSuccess(int pageNumber, int pageSize)
        {
            // Act
            var result = PaginationValidator.Validate(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Pagination parameters are valid.");
        }

        [Theory]
        [InlineData(0, 10, "Page number must be greater than 0.")]
        [InlineData(-1, 10, "Page number must be greater than 0.")]
        [InlineData(1, 0, "Page size must be greater than 0.")]
        [InlineData(1, -5, "Page size must be greater than 0.")]
        [InlineData(1, 51, "Page size cannot exceed 50.")]
        public void Validate_WhenInvalidInputs_ReturnsFailure(int pageNumber, int pageSize, string expectedErrorMessage)
        {
            // Act
            var result = PaginationValidator.Validate(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be(expectedErrorMessage);
        }
    }
}
