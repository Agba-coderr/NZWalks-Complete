using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NZWalks.APIs.CustomActionFilters;
using NZWalks.Application.DTOs;
using Xunit;

namespace NZWalks.API.Tests.CustomActionFilters
{
    public class ValidateFileUploadAttributeTests
    {
        private ActionExecutingContext CreateContext(Dictionary<string, object?> actionArguments)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor(),
                new ModelStateDictionary()
            );

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments,
                new Mock<Controller>().Object
            );
        }

        [Fact]
        public void OnActionExecuting_WhenNoDtoOrFileStreamNull_SetsBadRequestWithModelError()
        {
            // Arrange
            var context = CreateContext(new Dictionary<string, object?>
            {
                { "request", new ImageUploadRequestDto { FileStream = null!, FileName = "test.jpg" } }
            });
            var filter = new ValidateFileUploadAttribute();

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.Result.Should().NotBeNull();
            var badRequest = context.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var modelState = badRequest.Value.Should().BeOfType<SerializableError>().Subject;
            modelState.Should().ContainKey("File");
        }

        [Fact]
        public void OnActionExecuting_WhenDisallowedExtension_SetsBadRequestWithModelError()
        {
            // Arrange
            using var stream = new MemoryStream(new byte[100]);
            var requestDto = new ImageUploadRequestDto
            {
                FileStream = stream,
                FileName = "document.pdf",
                FileDescription = "PDF Document"
            };

            var context = CreateContext(new Dictionary<string, object?>
            {
                { "request", requestDto }
            });
            var filter = new ValidateFileUploadAttribute();

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.Result.Should().NotBeNull();
            var badRequest = context.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var modelState = badRequest.Value.Should().BeOfType<SerializableError>().Subject;
            modelState.Should().ContainKey("File");
        }

        [Fact]
        public void OnActionExecuting_WhenFileSizeExceeds10MB_SetsBadRequestWithModelError()
        {
            // Arrange (10MB + 1 byte = 10485761 bytes)
            var mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.Length).Returns(10485761);

            var requestDto = new ImageUploadRequestDto
            {
                FileStream = mockStream.Object,
                FileName = "large-image.jpg",
                FileDescription = "Huge photo"
            };

            var context = CreateContext(new Dictionary<string, object?>
            {
                { "request", requestDto }
            });
            var filter = new ValidateFileUploadAttribute();

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.Result.Should().NotBeNull();
            var badRequest = context.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var modelState = badRequest.Value.Should().BeOfType<SerializableError>().Subject;
            modelState.Should().ContainKey("File");
        }

        [Theory]
        [InlineData("image.jpg")]
        [InlineData("image.jpeg")]
        [InlineData("image.png")]
        [InlineData("IMAGE.PNG")]
        public void OnActionExecuting_WhenFileIsValid_DoesNotSetResult(string fileName)
        {
            // Arrange
            using var stream = new MemoryStream(new byte[1024]);
            var requestDto = new ImageUploadRequestDto
            {
                FileStream = stream,
                FileName = fileName,
                FileDescription = "Valid photo"
            };

            var context = CreateContext(new Dictionary<string, object?>
            {
                { "request", requestDto }
            });
            var filter = new ValidateFileUploadAttribute();

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.Result.Should().BeNull();
            context.ModelState.IsValid.Should().BeTrue();
        }
    }
}
