using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NZWalks.APIs.Controllers;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Regions.Commands;
using NZWalks.Application.Regions.Queries;
using Xunit;

namespace NZWalks.API.Tests.Controllers
{
    public class RegionsControllerTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly RegionsController _controller;

        public RegionsControllerTests()
        {
            _mockSender = new Mock<ISender>();
            _controller = new RegionsController(_mockSender.Object);
        }

        [Fact]
        public async Task GetAllRegions_SendsQueryAndReturnsStatusCode()
        {
            // Arrange
            var query = new GetAllRegionsQuery(PageNumber: 1, PageSize: 10);
            var pagedResponse = PagedResponse<RegionDto>.Create(new List<RegionDto>(), 1, 10, 0);
            var expectedResult = Result.Success(pagedResponse, "Success", 200);

            _mockSender.Setup(s => s.Send(query, default))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.GetAllRegions(query);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockSender.Verify(s => s.Send(query, default), Times.Once);
        }

        [Fact]
        public async Task GetRegionById_SendsQueryAndReturnsStatusCode()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var expectedResult = Result.Success(new RegionDto { Id = regionId, Code = "AKL", Name = "Auckland" }, "Found", 200);

            _mockSender.Setup(s => s.Send(It.Is<GetRegionByIdQuery>(q => q.Id == regionId), default))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.GetRegionById(regionId);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockSender.Verify(s => s.Send(It.Is<GetRegionByIdQuery>(q => q.Id == regionId), default), Times.Once);
        }

        [Fact]
        public async Task CreateRegion_SendsCommandAndReturnsStatusCode()
        {
            // Arrange
            var command = new CreateRegionCommand("AKL", "Auckland", "https://example.com/akl.jpg");
            var expectedResult = Result.Success(new RegionDto { Id = Guid.NewGuid(), Code = "AKL", Name = "Auckland" }, "Created", 201);

            _mockSender.Setup(s => s.Send(command, default))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.CreateRegion(command);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockSender.Verify(s => s.Send(command, default), Times.Once);
        }

        [Fact]
        public async Task UpdateRegion_SendsCommandWithRouteIdAndReturnsStatusCode()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var command = new UpdateRegionCommand(Guid.Empty, "AKL", "Auckland Updated", "https://example.com/akl.jpg");
            var expectedResult = Result.Success(new RegionDto { Id = regionId, Code = "AKL", Name = "Auckland Updated" }, "Updated", 200);

            _mockSender.Setup(s => s.Send(It.Is<UpdateRegionCommand>(c => c.Id == regionId && c.Name == "Auckland Updated"), default))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.UpdateRegion(regionId, command);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockSender.Verify(s => s.Send(It.Is<UpdateRegionCommand>(c => c.Id == regionId), default), Times.Once);
        }

        [Fact]
        public async Task DeleteRegion_SendsCommandAndReturnsStatusCode()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var expectedResult = Result.Success(new RegionDto { Id = regionId, Code = "AKL", Name = "Auckland" }, "Deleted", 200);

            _mockSender.Setup(s => s.Send(It.Is<DeleteRegionCommand>(c => c.Id == regionId), default))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.DeleteRegion(regionId);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockSender.Verify(s => s.Send(It.Is<DeleteRegionCommand>(c => c.Id == regionId), default), Times.Once);
        }
    }
}
