using AutoMapper;
using FluentAssertions;
using Moq;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Regions.Queries;
using NZWalks.Domain.Entities;
using Xunit;

namespace NZWalks.API.Tests.Regions.Queries
{
    public class GetRegionByIdQueryHandlerTests
    {
        private readonly Mock<IRegionRepository> _mockRegionRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetRegionByIdQueryHandler _handler;

        public GetRegionByIdQueryHandlerTests()
        {
            _mockRegionRepository = new Mock<IRegionRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetRegionByIdQueryHandler(_mockRegionRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WhenRegionNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var query = new GetRegionByIdQuery(regionId);

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync((Region?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Region with ID {regionId} was not found");
            Assert.Null(result.Data);

            _mockMapper.Verify(m => m.Map<RegionDto>(It.IsAny<Region>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRegionFound_ReturnsSuccessResultWithRegionDto()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var query = new GetRegionByIdQuery(regionId);

            var region = new Region
            {
                Id = regionId,
                Code = "WLG",
                Name = "Wellington",
                RegionImageUrl = "https://example.com/wellington.jpg"
            };

            var expectedDto = new RegionDto
            {
                Id = regionId,
                Code = "WLG",
                Name = "Wellington",
                RegionImageUrl = "https://example.com/wellington.jpg"
            };

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync(region);

            _mockMapper.Setup(m => m.Map<RegionDto>(region))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Region retrieved successfully");

            var data = result.Data as RegionDto;
            data.Should().NotBeNull();
            data.Should().BeEquivalentTo(expectedDto);

            _mockRegionRepository.Verify(r => r.GetRegionByIdAsync(regionId), Times.Once);
        }
    }
}
