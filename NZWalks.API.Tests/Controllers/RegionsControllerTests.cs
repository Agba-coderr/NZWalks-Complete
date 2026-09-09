using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NZWalks.APIs.Controllers;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Services;
using Xunit;

namespace NZWalks.API.Tests.Controllers
{
    public class RegionsControllerTests
    {
        private readonly Mock<IRegionService> _mockRegionService;
        private readonly RegionsController _controller;

        public RegionsControllerTests()
        {
            _mockRegionService = new Mock<IRegionService>();
            _controller = new RegionsController(_mockRegionService.Object);
        }

        [Fact]
        public async Task GetAllRegions_CallsServiceAndReturnsStatusCode()
        {
            // Arrange
            var expectedResult = Result.Success(new List<RegionDto>(), "Success", 200);
            _mockRegionService.Setup(s => s.GetAllRegionsAsync(1, 10))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.GetAllRegions(1, 10);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockRegionService.Verify(s => s.GetAllRegionsAsync(1, 10), Times.Once);
        }

        [Fact]
        public async Task GetRegionById_CallsServiceAndReturnsStatusCode()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var expectedResult = Result.Success(new RegionDto { Id = regionId, Code = "AKL", Name = "Auckland" }, "Found", 200);
            _mockRegionService.Setup(s => s.GetRegionByIdAsync(regionId))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.GetRegionById(regionId);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task CreateRegion_CallsServiceAndReturnsCreatedStatusCode()
        {
            // Arrange
            var requestDto = new AddRegionRequestDto { Code = "AKL", Name = "Auckland" };
            var expectedResult = Result.Success(new RegionDto { Id = Guid.NewGuid(), Code = "AKL", Name = "Auckland" }, "Created", 201);

            _mockRegionService.Setup(s => s.CreateRegionAsync(requestDto))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.CreateRegion(requestDto);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task UpdateRegion_CallsServiceAndReturnsStatusCode()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var updateDto = new UpdateRegionDto { Code = "AKL", Name = "Auckland Updated" };
            var expectedResult = Result.Success(new RegionDto { Id = regionId, Code = "AKL", Name = "Auckland Updated" }, "Updated", 200);

            _mockRegionService.Setup(s => s.UpdateRegionAsync(regionId, updateDto))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.UpdateRegion(regionId, updateDto);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task DeleteRegion_CallsServiceAndReturnsStatusCode()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var expectedResult = Result.Success(new RegionDto { Id = regionId, Code = "AKL", Name = "Auckland" }, "Deleted", 200);

            _mockRegionService.Setup(s => s.DeleteRegionAsync(regionId))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.DeleteRegion(regionId);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
        }
    }
}
