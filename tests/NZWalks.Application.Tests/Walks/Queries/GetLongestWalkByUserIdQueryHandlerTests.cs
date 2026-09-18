using AutoMapper;
using FluentAssertions;
using Moq;
using NZWalks.API.Tests.Common;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using NZWalks.Application.Walks.Queries;
using NZWalks.Domain.Entities;
using Xunit;

namespace NZWalks.API.Tests.Walks.Queries
{
    public class GetLongestWalkByUserIdQueryHandlerTests
    {
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly GetLongestWalkByUserIdQueryHandler _handler;

        public GetLongestWalkByUserIdQueryHandlerTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _handler = new GetLongestWalkByUserIdQueryHandler(_mockWalkRepository.Object, _mockMapper.Object, _mockCurrentUser.Object);
        }

        [Fact]
        public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorizedResult()
        {
            // Arrange
            _mockCurrentUser.Setup(u => u.UserId).Returns(string.Empty);
            var query = new GetLongestWalkByUserIdQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(401);
            result.Message.Should().Be("User is not authenticated");

            _mockWalkRepository.Verify(r => r.GetLongestWalkByUserIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenWalkDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var userId = "user-123";
            _mockCurrentUser.Setup(u => u.UserId).Returns(userId);
            _mockWalkRepository.Setup(r => r.GetLongestWalkByUserIdAsync(userId))
                .ReturnsAsync((Walk?)null);

            var query = new GetLongestWalkByUserIdQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Longest walk not found for user: {userId}");
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_WhenWalkExists_ReturnsSuccessResultWithWalkDto()
        {
            // Arrange
            var userId = "user-123";
            var longestWalk = TestDataHelper.CreateWalk(name: "Milford Track", lengthInKm: 53.5, userId: userId);
            var expectedDto = TestDataHelper.CreateWalkDto(longestWalk.Id, name: "Milford Track", lengthInKm: 53.5);

            _mockCurrentUser.Setup(u => u.UserId).Returns(userId);
            _mockWalkRepository.Setup(r => r.GetLongestWalkByUserIdAsync(userId))
                .ReturnsAsync(longestWalk);
            _mockMapper.Setup(m => m.Map<WalkDto>(longestWalk))
                .Returns(expectedDto);

            var query = new GetLongestWalkByUserIdQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be($"Longest walk for user: {userId} retrieved successfully");

            var data = result.Data as WalkDto;
            data.Should().NotBeNull();
            data.Should().BeEquivalentTo(expectedDto);

            _mockWalkRepository.Verify(r => r.GetLongestWalkByUserIdAsync(userId), Times.Once);
        }
    }
}
