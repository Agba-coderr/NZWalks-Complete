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
    public class CreateWalkCommandHandlerTests
    {
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<IRegionRepository> _mockRegionRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CreateWalkCommandHandler _handler;

        public CreateWalkCommandHandlerTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockRegionRepository = new Mock<IRegionRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _handler = new CreateWalkCommandHandler(
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
            var command = new CreateWalkCommand("Walk", "Desc", 10.0, null, DifficultyType.Easy, Guid.NewGuid());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(401);
            result.Message.Should().Be("User is not authenticated");

            _mockRegionRepository.Verify(r => r.GetRegionByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockWalkRepository.Verify(w => w.CreateWalkAsync(It.IsAny<Walk>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRegionDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            _mockCurrentUser.Setup(u => u.UserId).Returns("user-123");
            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync((Region?)null);

            var command = new CreateWalkCommand("Walk", "Desc", 10.0, null, DifficultyType.Easy, regionId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Region ID {regionId} does not exist");

            _mockWalkRepository.Verify(w => w.CreateWalkAsync(It.IsAny<Walk>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenValid_CreatesWalkAndReturnsSuccessResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var userId = "user-123";
            var region = TestDataHelper.CreateRegion(regionId, code: "AKL", name: "Auckland");

            _mockCurrentUser.Setup(u => u.UserId).Returns(userId);
            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync(region);

            var command = new CreateWalkCommand("Mountain Track", "Beautiful mountain climb", 12.5, "https://example.com/walk.jpg", DifficultyType.Hard, regionId);

            var createdWalk = TestDataHelper.CreateWalk(
                name: command.Name,
                description: command.Description,
                lengthInKm: command.LengthInKm,
                difficulty: command.DifficultyType,
                regionId: regionId,
                region: region,
                userId: userId,
                imageUrl: command.WalkImageUrl);

            var expectedDto = TestDataHelper.CreateWalkDto(
                id: createdWalk.Id,
                name: createdWalk.Name,
                description: createdWalk.Description,
                lengthInKm: createdWalk.LengthInKm,
                difficulty: createdWalk.DifficultyType,
                regionId: regionId,
                imageUrl: createdWalk.WalkImageUrl);

            _mockWalkRepository.Setup(w => w.CreateWalkAsync(It.IsAny<Walk>()))
                .ReturnsAsync(createdWalk);

            _mockMapper.Setup(m => m.Map<WalkDto>(createdWalk))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Walk created successfully.");

            var data = result.Data as WalkDto;
            data.Should().NotBeNull();
            data.Should().BeEquivalentTo(expectedDto);

            _mockWalkRepository.Verify(w => w.CreateWalkAsync(It.Is<Walk>(walk =>
                walk.Name == command.Name &&
                walk.CreatedByUserId == userId &&
                walk.RegionId == regionId)), Times.Once);
        }
    }
}
