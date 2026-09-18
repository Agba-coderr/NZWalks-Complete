using AutoMapper;
using FluentAssertions;
using Moq;
using NZWalks.API.Tests.Common;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Walks.Queries;
using NZWalks.Domain.Entities;
using Xunit;

namespace NZWalks.API.Tests.Walks.Queries
{
    public class GetWalkByIdQueryHandlerTests
    {
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetWalkByIdQueryHandler _handler;

        public GetWalkByIdQueryHandlerTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetWalkByIdQueryHandler(_mockWalkRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WhenWalkDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync((Walk?)null);

            var query = new GetWalkByIdQuery(walkId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Walk with ID {walkId} not found.");
            Assert.Null(result.Data);

            _mockMapper.Verify(m => m.Map<WalkDto>(It.IsAny<Walk>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenWalkExists_ReturnsSuccessResultWithWalkDto()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var walk = TestDataHelper.CreateWalk(walkId, name: "Coast Walk", description: "Scenic coast walk", lengthInKm: 10.0);
            var expectedDto = TestDataHelper.CreateWalkDto(walkId, name: "Coast Walk", description: "Scenic coast walk", lengthInKm: 10.0);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(walk);

            _mockMapper.Setup(m => m.Map<WalkDto>(walk))
                .Returns(expectedDto);

            var query = new GetWalkByIdQuery(walkId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be($"Walk with ID {walkId} retrieved successfully");

            var data = result.Data as WalkDto;
            data.Should().NotBeNull();
            data.Should().BeEquivalentTo(expectedDto);

            _mockWalkRepository.Verify(r => r.GetWalkByIdAsync(walkId), Times.Once);
        }
    }
}
