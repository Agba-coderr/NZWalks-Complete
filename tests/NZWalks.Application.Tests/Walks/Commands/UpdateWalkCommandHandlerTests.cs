using AutoMapper;
using FluentAssertions;
using Moq;
using NZWalks.API.Tests.Common;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using NZWalks.Application.Walks.Commands;
using NZWalks.Domain.Entities;
using NZWalks.Domain.Enums;
using Xunit;

namespace NZWalks.API.Tests.Walks.Commands
{
    public class UpdateWalkCommandHandlerTests
    {
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<IRegionRepository> _mockRegionRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UpdateWalkCommandHandler _handler;

        public UpdateWalkCommandHandlerTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockRegionRepository = new Mock<IRegionRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _handler = new UpdateWalkCommandHandler(
                _mockWalkRepository.Object,
                _mockRegionRepository.Object,
                _mockCurrentUser.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorizedResult()
        {
            // Arrange
            _mockCurrentUser.Setup(u => u.UserId).Returns((string?)null);
            var command = new UpdateWalkCommand(Guid.NewGuid(), "Walk", "Desc", 5.0, null, DifficultyType.Easy, Guid.NewGuid());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(401);
            result.Message.Should().Be("User is not authenticated");
        }

        [Fact]
        public async Task Handle_WhenWalkDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            _mockCurrentUser.Setup(u => u.UserId).Returns("user-123");
            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync((Walk?)null);

            var command = new UpdateWalkCommand(walkId, "Walk", "Desc", 5.0, null, DifficultyType.Easy, Guid.NewGuid());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("This walk does not exist.");
        }

        [Fact]
        public async Task Handle_WhenUserIsNotOwner_ReturnsForbiddenResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var existingWalk = TestDataHelper.CreateWalk(walkId, userId: "owner-id");

            _mockCurrentUser.Setup(u => u.UserId).Returns("other-user-id");
            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            var command = new UpdateWalkCommand(walkId, "Walk", "Desc", 5.0, null, DifficultyType.Easy, Guid.NewGuid());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(403);
            result.Message.Should().Be("You do not own this walk.");
        }

        [Fact]
        public async Task Handle_WhenRegionDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-id";
            var regionId = Guid.NewGuid();
            var existingWalk = TestDataHelper.CreateWalk(walkId, userId: userId);

            _mockCurrentUser.Setup(u => u.UserId).Returns(userId);
            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);
            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync((Region?)null);

            var command = new UpdateWalkCommand(walkId, "Walk", "Desc", 5.0, null, DifficultyType.Easy, regionId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("Invalid region ID.");
        }

        [Fact]
        public async Task Handle_WhenRepositoryUpdateReturnsNull_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-id";
            var regionId = Guid.NewGuid();
            var region = TestDataHelper.CreateRegion(regionId, code: "AKL", name: "Auckland");
            var existingWalk = TestDataHelper.CreateWalk(walkId, userId: userId);

            var command = new UpdateWalkCommand(walkId, "Walk", "Desc", 5.0, null, DifficultyType.Easy, regionId);
            var domainModel = TestDataHelper.CreateWalk(name: "Walk", description: "Desc", lengthInKm: 5.0, regionId: regionId, region: region, userId: userId);

            _mockCurrentUser.Setup(u => u.UserId).Returns(userId);
            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);
            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync(region);
            _mockMapper.Setup(m => m.Map<Walk>(command))
                .Returns(domainModel);
            _mockWalkRepository.Setup(r => r.UpdateWalkAsync(walkId, It.IsAny<Walk>()))
                .ReturnsAsync((Walk?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Walk with ID {walkId} not found.");
        }

        [Fact]
        public async Task Handle_WhenValid_UpdatesWalkAndReturnsSuccessResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-id";
            var regionId = Guid.NewGuid();
            var region = TestDataHelper.CreateRegion(regionId, code: "AKL", name: "Auckland");
            var existingWalk = TestDataHelper.CreateWalk(walkId, userId: userId);

            var command = new UpdateWalkCommand(walkId, "Updated Walk", "Updated Desc", 12.0, "https://example.com/walk.jpg", DifficultyType.Hard, regionId);
            var domainModel = TestDataHelper.CreateWalk(name: command.Name, description: command.Description, lengthInKm: command.LengthInKm, difficulty: command.DifficultyType, regionId: regionId, region: region, userId: userId);
            var updatedWalk = TestDataHelper.CreateWalk(walkId, name: command.Name, description: command.Description, lengthInKm: command.LengthInKm, difficulty: command.DifficultyType, regionId: regionId, region: region, userId: userId);

            var expectedDto = TestDataHelper.CreateWalkDto(walkId, name: command.Name);

            _mockCurrentUser.Setup(u => u.UserId).Returns(userId);
            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);
            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync(region);
            _mockMapper.Setup(m => m.Map<Walk>(command))
                .Returns(domainModel);
            _mockWalkRepository.Setup(r => r.UpdateWalkAsync(walkId, It.IsAny<Walk>()))
                .ReturnsAsync(updatedWalk);
            _mockMapper.Setup(m => m.Map<WalkDto>(updatedWalk))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Walk updated successfully");

            var data = result.Data as WalkDto;
            data.Should().NotBeNull();
            data.Should().BeEquivalentTo(expectedDto);

            _mockWalkRepository.Verify(r => r.UpdateWalkAsync(walkId, It.Is<Walk>(w =>
                w.Region == region &&
                w.CreatedByUserId == userId)), Times.Once);
        }
    }
}
