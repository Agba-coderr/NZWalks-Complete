using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NZWalks.API.Tests.Common;
using NZWalks.APIs.Controllers;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Walks.Commands;
using NZWalks.Application.Walks.Queries;
using NZWalks.Domain.Enums;
using Xunit;

namespace NZWalks.API.Tests.Controllers
{
    public class WalksControllerTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly WalksController _controller;

        public WalksControllerTests()
        {
            _mockSender = new Mock<ISender>();
            _controller = new WalksController(_mockSender.Object);
        }

        [Fact]
        public async Task GetAllWalks_SendsQueryAndReturnsStatusCode()
        {
            // Arrange
            var query = new GetAllWalksQuery(FilterOn: null, FilterQuery: null, PageNumber: 1, PageSize: 10);
            var expectedResponse = Result.Success(new List<WalkDto>(), "Success", 200);

            _mockSender.Setup(s => s.Send(query, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.GetAllWalks(query);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockSender.Verify(s => s.Send(query, default), Times.Once);
        }

        [Fact]
        public async Task GetWalkById_SendsQueryAndReturnsStatusCode()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var expectedResponse = Result.Success(TestDataHelper.CreateWalkDto(walkId, name: "Walk 1"), "Found", 200);

            _mockSender.Setup(s => s.Send(It.Is<GetWalkByIdQuery>(q => q.Id == walkId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.GetWalkById(walkId);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockSender.Verify(s => s.Send(It.Is<GetWalkByIdQuery>(q => q.Id == walkId), default), Times.Once);
        }

        [Fact]
        public async Task GetWalksByUserId_SendsQueryAndReturnsStatusCode()
        {
            // Arrange
            var query = new GetWalksByUserIdQuery(PageNumber: 1, PageSize: 10);
            var expectedResponse = Result.Success(new List<WalkDto>(), "Success", 200);

            _mockSender.Setup(s => s.Send(query, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.GetWalksByUserId(query);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockSender.Verify(s => s.Send(query, default), Times.Once);
        }

        [Fact]
        public async Task GetLongestWalkByUserId_SendsQueryAndReturnsStatusCode()
        {
            // Arrange
            var expectedResponse = Result.Success(TestDataHelper.CreateWalkDto(name: "Longest Walk"), "Success", 200);

            _mockSender.Setup(s => s.Send(It.IsAny<GetLongestWalkByUserIdQuery>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.GetLongestWalkByUserId();

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockSender.Verify(s => s.Send(It.IsAny<GetLongestWalkByUserIdQuery>(), default), Times.Once);
        }

        [Fact]
        public async Task GetWalksByRegionId_SendsQueryWithRegionIdAndReturnsStatusCode()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var query = new GetWalksByRegionIdQuery(RegionId: Guid.Empty, PageNumber: 1, PageSize: 10);
            var expectedResponse = Result.Success(new List<WalkDto>(), "Success", 200);

            _mockSender.Setup(s => s.Send(It.Is<GetWalksByRegionIdQuery>(q => q.RegionId == regionId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.GetWalksByRegionId(regionId, query);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockSender.Verify(s => s.Send(It.Is<GetWalksByRegionIdQuery>(q => q.RegionId == regionId), default), Times.Once);
        }

        [Fact]
        public async Task GetWalksByDifficulty_SendsQueryAndReturnsStatusCode()
        {
            // Arrange
            var query = new GetWalksByDifficultyQuery(Difficulty: DifficultyType.Easy, PageNumber: 1, PageSize: 10);
            var expectedResponse = Result.Success(new List<WalkDto>(), "Success", 200);

            _mockSender.Setup(s => s.Send(query, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.GetWalksByDifficulty(query);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockSender.Verify(s => s.Send(query, default), Times.Once);
        }

        [Fact]
        public async Task CreateWalk_SendsCommandAndReturnsStatusCode()
        {
            // Arrange
            var command = new CreateWalkCommand("Walk", "Desc", 10.5, "https://example.com/walk.jpg", DifficultyType.Easy, Guid.NewGuid());
            var expectedResponse = Result.Success(TestDataHelper.CreateWalkDto(name: "Walk"), "Created", 201);

            _mockSender.Setup(s => s.Send(command, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.CreateWalk(command);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockSender.Verify(s => s.Send(command, default), Times.Once);
        }

        [Fact]
        public async Task UpdateWalk_SendsCommandWithRouteIdAndReturnsStatusCode()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var command = new UpdateWalkCommand(Guid.Empty, "Updated Walk", "Desc", 12.0, "https://example.com/walk.jpg", DifficultyType.Hard, Guid.NewGuid());
            var expectedResponse = Result.Success(TestDataHelper.CreateWalkDto(walkId, name: "Updated Walk"), "Updated", 200);

            _mockSender.Setup(s => s.Send(It.Is<UpdateWalkCommand>(c => c.Id == walkId && c.Name == "Updated Walk"), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.UpdateWalk(walkId, command);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockSender.Verify(s => s.Send(It.Is<UpdateWalkCommand>(c => c.Id == walkId), default), Times.Once);
        }

        [Fact]
        public async Task DeleteWalk_SendsCommandAndReturnsStatusCode()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var expectedResponse = Result.Success(TestDataHelper.CreateWalkDto(walkId, name: "Deleted Walk"), "Deleted", 200);

            _mockSender.Setup(s => s.Send(It.Is<DeleteWalkCommand>(c => c.Id == walkId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.DeleteWalk(walkId);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockSender.Verify(s => s.Send(It.Is<DeleteWalkCommand>(c => c.Id == walkId), default), Times.Once);
        }
    }
}
