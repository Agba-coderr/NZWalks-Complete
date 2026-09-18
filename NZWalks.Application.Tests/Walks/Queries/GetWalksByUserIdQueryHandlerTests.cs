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
    public class GetWalksByUserIdQueryHandlerTests
    {
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly GetWalksByUserIdQueryHandler _handler;

        public GetWalksByUserIdQueryHandlerTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockCurrentUser = new Mock<ICurrentUserService>();

            _handler = new GetWalksByUserIdQueryHandler(
                _mockWalkRepository.Object,
                _mockMapper.Object,
                _mockCurrentUser.Object);
        }

        [Fact]
        public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorizedResult()
        {
            // Arrange
            _mockCurrentUser.Setup(u => u.UserId).Returns(string.Empty);
            var query = new GetWalksByUserIdQuery(PageNumber: 1, PageSize: 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(401);
            result.Message.Should().Be("User is not authenticated");

            _mockWalkRepository.Verify(
                r => r.GetWalksByUserIdAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenInvalidPagination_ReturnsBadRequest()
        {
            // Arrange
            _mockCurrentUser.Setup(u => u.UserId).Returns("user-123");
            var query = new GetWalksByUserIdQuery(PageNumber: 1, PageSize: 55);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);

            _mockWalkRepository.Verify(
                r => r.GetWalksByUserIdAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenValid_ReturnsPagedSuccessResult()
        {
            // Arrange
            var userId = "user-123";
            var query = new GetWalksByUserIdQuery(PageNumber: 1, PageSize: 10);

            var walks = new List<Walk>
            {
                TestDataHelper.CreateWalk(name: "Walk 1", userId: userId)
            };

            var dtoList = new List<WalkDto>
            {
                TestDataHelper.CreateWalkDto(walks[0].Id, name: "Walk 1")
            };

            _mockCurrentUser.Setup(u => u.UserId).Returns(userId);
            _mockWalkRepository.Setup(r => r.GetWalksByUserIdAsync(userId, 1, 10))
                .ReturnsAsync((walks, 1));

            _mockMapper.Setup(m => m.Map<List<WalkDto>>(walks))
                .Returns(dtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be($"Walks for the user: {userId} retrieved successfully");

            var pagedResponse = result.Data as PagedResponse<WalkDto>;
            pagedResponse.Should().NotBeNull();
            pagedResponse!.Data.Should().HaveCount(1);

            _mockWalkRepository.Verify(r => r.GetWalksByUserIdAsync(userId, 1, 10), Times.Once);
        }
    }
}
