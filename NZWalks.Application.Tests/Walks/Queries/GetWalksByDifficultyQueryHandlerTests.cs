using AutoMapper;
using FluentAssertions;
using Moq;
using NZWalks.API.Tests.Common;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Walks.Queries;
using NZWalks.Domain.Entities;
using NZWalks.Domain.Enums;
using Xunit;

namespace NZWalks.API.Tests.Walks.Queries
{
    public class GetWalksByDifficultyQueryHandlerTests
    {
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetWalksByDifficultyQueryHandler _handler;

        public GetWalksByDifficultyQueryHandlerTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetWalksByDifficultyQueryHandler(_mockWalkRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WhenInvalidPagination_ReturnsBadRequestWithoutCallingRepo()
        {
            // Arrange
            var query = new GetWalksByDifficultyQuery(DifficultyType.Easy, PageNumber: 0, PageSize: 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);

            _mockWalkRepository.Verify(
                r => r.GetWalksByDifficultyAsync(It.IsAny<DifficultyType>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenValid_ReturnsPagedSuccessResult()
        {
            // Arrange
            var difficulty = DifficultyType.Moderate;
            int pageNumber = 1;
            int pageSize = 10;
            var query = new GetWalksByDifficultyQuery(difficulty, pageNumber, pageSize);

            var walks = new List<Walk>
            {
                TestDataHelper.CreateWalk(name: "Walk 1", difficulty: difficulty)
            };

            var dtoList = new List<WalkDto>
            {
                TestDataHelper.CreateWalkDto(walks[0].Id, name: "Walk 1", difficulty: difficulty)
            };

            _mockWalkRepository.Setup(r => r.GetWalksByDifficultyAsync(difficulty, pageNumber, pageSize))
                .ReturnsAsync((walks, 1));

            _mockMapper.Setup(m => m.Map<List<WalkDto>>(walks))
                .Returns(dtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be($"Walks with difficulty: {difficulty} retrieved successfully");

            var pagedResponse = result.Data as PagedResponse<WalkDto>;
            pagedResponse.Should().NotBeNull();
            pagedResponse!.Data.Should().HaveCount(1);

            _mockWalkRepository.Verify(r => r.GetWalksByDifficultyAsync(difficulty, pageNumber, pageSize), Times.Once);
        }
    }
}
