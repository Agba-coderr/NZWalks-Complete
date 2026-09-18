using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NZWalks.APIs.CustomActionFilters;
using Xunit;

namespace NZWalks.API.Tests.CustomActionFilters
{
    public class ValidateModelAttributeTests
    {
        [Fact]
        public void OnActionExecuted_WhenModelStateIsInvalid_SetsBadRequestResult()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Name", "Name is required");

            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor(),
                modelState
            );

            var context = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Mock<Controller>().Object
            );

            var filter = new ValidateModelAttribute();

            // Act
            filter.OnActionExecuted(context);

            // Assert
            context.Result.Should().NotBeNull();
            context.Result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void OnActionExecuted_WhenModelStateIsValid_DoesNotSetResult()
        {
            // Arrange
            var modelState = new ModelStateDictionary();

            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor(),
                modelState
            );

            var context = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Mock<Controller>().Object
            );

            var filter = new ValidateModelAttribute();

            // Act
            filter.OnActionExecuted(context);

            // Assert
            context.Result.Should().BeNull();
        }
    }
}
