using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NZWalks.APIs.Controllers;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Domain.Enums;
using NZWalks.Application.Interfaces.Services;
using System.Security.Claims;
using Xunit;

namespace NZWalks.API.Tests.Controllers
{
    public class WalksControllerTests
    {
        private readonly Mock<IWalkService> _mockWalkService;
        private readonly WalksController _controller;

        public WalksControllerTests()
        {
            _mockWalkService = new Mock<IWalkService>();
            _controller = new WalksController(_mockWalkService.Object);
        }

        private void SetControllerUser(string? userId, string[]? roles = null)
        {
            var claims = new List<Claim>();
            if (userId != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, userId != null ? "TestAuth" : null);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetAllWalks_CallsServiceAndReturnsStatusCode()
        {
            // Arrange
            var expectedResponse = Result.Success(new List<WalkDto>(), "Success", 200);
            _mockWalkService.Setup(s => s.GetAllWalksAsync(null, null, 1, 10))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.GetAllWalks(null, null, 1, 10);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetWalkById_CallsServiceAndReturnsStatusCode()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var expectedResponse = Result.Success(new WalkDto
            {
                Id = walkId,
                Name = "Walk 1",
                Description = "Desc",
                DifficultyType = DifficultyType.Easy,
                Region = new RegionDto { Code = "AKL", Name = "Auckland" }
            }, "Found", 200);

            _mockWalkService.Setup(s => s.GetWalkByIdAsync(walkId))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.GetWalkById(walkId);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetWalksByUserId_WhenUnauthenticated_ReturnsUnauthorized()
        {
            // Arrange
            SetControllerUser(userId: null);

            // Act
            var actionResult = await _controller.GetWalksByUserId(1, 10);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task GetWalksByUserId_WhenAuthenticated_CallsService()
        {
            // Arrange
            var userId = "user-123";
            SetControllerUser(userId);
            var expectedResponse = Result.Success(new List<WalkDto>(), "Success", 200);

            _mockWalkService.Setup(s => s.GetWalksByUserIdAsync(userId, 1, 10))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.GetWalksByUserId(1, 10);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task CreateWalk_WhenUnauthenticated_ReturnsUnauthorized()
        {
            // Arrange
            SetControllerUser(userId: null);
            var requestDto = new AddWalkRequestDto
            {
                Name = "Walk",
                Description = "Desc",
                DifficultyType = DifficultyType.Easy,
                RegionId = Guid.NewGuid()
            };

            // Act
            var actionResult = await _controller.CreateWalk(requestDto);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task CreateWalk_WhenAuthenticated_CallsServiceWithUserId()
        {
            // Arrange
            var userId = "user-abc";
            SetControllerUser(userId);
            var requestDto = new AddWalkRequestDto
            {
                Name = "Walk",
                Description = "Desc",
                DifficultyType = DifficultyType.Easy,
                RegionId = Guid.NewGuid()
            };
            var expectedResponse = Result.Success(null, "Created", 201);

            _mockWalkService.Setup(s => s.CreateWalkAsync(requestDto, userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.CreateWalk(requestDto);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task UpdateWalk_WhenAuthenticated_PassesAdminFlagAndUserId()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "admin-user";
            SetControllerUser(userId, roles: new[] { "Admin" });

            var updateDto = new UpdateWalkDto
            {
                Name = "Updated Walk",
                Description = "Desc",
                DifficultyType = DifficultyType.Hard,
                RegionId = Guid.NewGuid()
            };
            var expectedResponse = Result.Success(null, "Updated", 200);

            _mockWalkService.Setup(s => s.UpdateWalkAsync(walkId, updateDto, userId, true))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.UpdateWalk(walkId, updateDto);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            _mockWalkService.Verify(s => s.UpdateWalkAsync(walkId, updateDto, userId, true), Times.Once);
        }

        [Fact]
        public async Task DeleteWalk_WhenAuthenticated_PassesAdminFlagAndUserId()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "writer-user";
            SetControllerUser(userId, roles: new[] { "Writer" });
            var expectedResponse = Result.Success(null, "Deleted", 200);

            _mockWalkService.Setup(s => s.DeleteWalkAsync(walkId, userId, false))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.DeleteWalk(walkId);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            _mockWalkService.Verify(s => s.DeleteWalkAsync(walkId, userId, false), Times.Once);
        }
    }
}
