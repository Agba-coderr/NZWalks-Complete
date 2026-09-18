using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NZWalks.Infrastructure.Services;
using System.Security.Claims;
using Xunit;

namespace NZWalks.API.Tests.Services
{
    public class CurrentUserServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly CurrentUserService _service;

        public CurrentUserServiceTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _service = new CurrentUserService(_mockHttpContextAccessor.Object);
        }

        [Fact]
        public void Properties_WhenHttpContextIsNull_ReturnsDefaults()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns((HttpContext?)null);

            // Assert
            _service.UserId.Should().BeNull();
            _service.IsAuthenticated.Should().BeFalse();
            _service.IsAdmin.Should().BeFalse();
        }

        [Fact]
        public void Properties_WhenUserIsAuthenticatedAndAdmin_ReturnsExpectedValues()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "user-456"),
                new(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };

            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            // Assert
            _service.UserId.Should().Be("user-456");
            _service.IsAuthenticated.Should().BeTrue();
            _service.IsAdmin.Should().BeTrue();
        }

        [Fact]
        public void Properties_WhenUserIsAuthenticatedNonAdmin_ReturnsExpectedValues()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "user-789"),
                new(ClaimTypes.Role, "Reader")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };

            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            // Assert
            _service.UserId.Should().Be("user-789");
            _service.IsAuthenticated.Should().BeTrue();
            _service.IsAdmin.Should().BeFalse();
        }
    }
}
