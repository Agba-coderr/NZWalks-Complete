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
    public class GetWalksByRegionIdQueryHandlerTests
    {
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<IRegionRepository> _mockRegionRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetWalksByRegionIdQueryHandler _handler;

        public GetWalksByRegionIdQueryHandlerTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockRegionRepository = new Mock<IRegionRepository>();
            _mockMapper = new Mock<IMapper>();

            _handler = new GetWalksByRegionIdQueryHandler(
                _mockWalkRepository.Object,
                _mockRegionRepository.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WhenInvalidPagination_ReturnsBadRequestWithoutQueryingDatabase()
        {
            // Arrange
            var query = new GetWalksByRegionIdQuery(Guid.NewGuid(), PageNumber: -1, PageSize: 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);

            _mockRegionRepository.Verify(r => r.GetRegionByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockWalkRepository.Verify(
                w => w.GetWalksByRegionIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRegionDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var query = new GetWalksByRegionIdQuery(regionId, PageNumber: 1, PageSize: 10);

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync((Region?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Region with ID {regionId} does not exist.");

            _mockWalkRepository.Verify(
                w => w.GetWalksByRegionIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRegionExists_ReturnsPagedSuccessResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var region = TestDataHelper.CreateRegion(regionId, code: "AKL", name: "Auckland");
            var query = new GetWalksByRegionIdQuery(regionId, PageNumber: 1, PageSize: 10);

            var walks = new List<Walk>
            {
                TestDataHelper.CreateWalk(name: "Walk 1", regionId: regionId, region: region)
            };

            var dtoList = new List<WalkDto>
            {
                TestDataHelper.CreateWalkDto(walks[0].Id, name: "Walk 1", regionId: regionId)
            };

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync(region);

            _mockWalkRepository.Setup(w => w.GetWalksByRegionIdAsync(regionId, 1, 10))
                .ReturnsAsync((walks, 1));

            _mockMapper.Setup(m => m.Map<List<WalkDto>>(walks))
                .Returns(dtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be($"Walks for the region: {regionId} retrieved successfully");

            var pagedResponse = result.Data as PagedResponse<WalkDto>;
            pagedResponse.Should().NotBeNull();
            pagedResponse!.Data.Should().HaveCount(1);
        }
    }
}
