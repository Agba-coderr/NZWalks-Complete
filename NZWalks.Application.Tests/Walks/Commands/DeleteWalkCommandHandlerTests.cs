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
using Xunit;

namespace NZWalks.API.Tests.Walks.Commands
{
    public class DeleteWalkCommandHandlerTests
    {
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DeleteWalkCommandHandler _handler;

        public DeleteWalkCommandHandlerTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _handler = new DeleteWalkCommandHandler(
                _mockWalkRepository.Object,
                _mockCurrentUser.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorizedResult()
        {
            // Arrange
            _mockCurrentUser.Setup(u => u.UserId).Returns(string.Empty);
            var command = new DeleteWalkCommand(Guid.NewGuid());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(401);
            result.Message.Should().Be("User is not authenticated");

            _mockWalkRepository.Verify(r => r.GetWalkByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenWalkDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            _mockCurrentUser.Setup(u => u.UserId).Returns("user-123");
            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync((Walk?)null);

            var command = new DeleteWalkCommand(walkId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("This walk does not exist.");

            _mockWalkRepository.Verify(r => r.DeleteWalkAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserIsNotOwnerAndNotAdmin_ReturnsForbiddenResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var existingWalk = TestDataHelper.CreateWalk(walkId, name: "Coast Walk", userId: "owner-id");

            _mockCurrentUser.Setup(u => u.UserId).Returns("other-user-id");
            _mockCurrentUser.Setup(u => u.IsAdmin).Returns(false);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            var command = new DeleteWalkCommand(walkId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(403);
            result.Message.Should().Be("You do not own this walk.");

            _mockWalkRepository.Verify(r => r.DeleteWalkAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserIsOwner_DeletesSuccessfully()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-id";
            var existingWalk = TestDataHelper.CreateWalk(walkId, name: "Coast Walk", userId: userId);
            var expectedDto = TestDataHelper.CreateWalkDto(walkId, name: "Coast Walk");

            _mockCurrentUser.Setup(u => u.UserId).Returns(userId);
            _mockCurrentUser.Setup(u => u.IsAdmin).Returns(false);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockWalkRepository.Setup(r => r.DeleteWalkAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockMapper.Setup(m => m.Map<WalkDto>(existingWalk))
                .Returns(expectedDto);

            var command = new DeleteWalkCommand(walkId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Walk deleted successfully");

            _mockWalkRepository.Verify(r => r.DeleteWalkAsync(walkId), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenUserIsAdmin_DeletesSuccessfullyEvenIfNotOwner()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var existingWalk = TestDataHelper.CreateWalk(walkId, name: "Coast Walk", userId: "creator-id");
            var expectedDto = TestDataHelper.CreateWalkDto(walkId, name: "Coast Walk");

            _mockCurrentUser.Setup(u => u.UserId).Returns("admin-id");
            _mockCurrentUser.Setup(u => u.IsAdmin).Returns(true);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockWalkRepository.Setup(r => r.DeleteWalkAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockMapper.Setup(m => m.Map<WalkDto>(existingWalk))
                .Returns(expectedDto);

            var command = new DeleteWalkCommand(walkId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Walk deleted successfully");

            _mockWalkRepository.Verify(r => r.DeleteWalkAsync(walkId), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryDeleteFails_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-id";
            var existingWalk = TestDataHelper.CreateWalk(walkId, name: "Coast Walk", userId: userId);

            _mockCurrentUser.Setup(u => u.UserId).Returns(userId);
            _mockCurrentUser.Setup(u => u.IsAdmin).Returns(false);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockWalkRepository.Setup(r => r.DeleteWalkAsync(walkId))
                .ReturnsAsync((Walk?)null);

            var command = new DeleteWalkCommand(walkId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("This walk no longer exists.");
        }
    }
}
