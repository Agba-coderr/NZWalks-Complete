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
    public class GetAllRegionsQueryHandlerTests
    {
        private readonly Mock<IRegionRepository> _mockRegionRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetAllRegionsQueryHandler _handler;

        public GetAllRegionsQueryHandlerTests()
        {
            _mockRegionRepository = new Mock<IRegionRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetAllRegionsQueryHandler(_mockRegionRepository.Object, _mockMapper.Object);
        }

        [Theory]
        [InlineData(0, 10, "Page number must be greater than 0.")]
        [InlineData(-1, 10, "Page number must be greater than 0.")]
        [InlineData(1, 0, "Page size must be greater than 0.")]
        [InlineData(1, -5, "Page size must be greater than 0.")]
        [InlineData(1, 51, "Page size cannot exceed 50.")]
        public async Task Handle_WhenInvalidPagination_ReturnsBadRequestWithoutCallingRepository(
            int pageNumber, int pageSize, string expectedErrorMessage)
        {
            // Arrange
            var query = new GetAllRegionsQuery(pageNumber, pageSize);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be(expectedErrorMessage);
            Assert.Null(result.Data);

            _mockRegionRepository.Verify(
                r => r.GetAllRegionsAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenValidPagination_ReturnsPagedSuccessResult()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            int totalCount = 2;
            var query = new GetAllRegionsQuery(pageNumber, pageSize);

            var domainRegions = new List<Region>
            {
                new() { Id = Guid.NewGuid(), Code = "AKL", Name = "Auckland" },
                new() { Id = Guid.NewGuid(), Code = "WLG", Name = "Wellington" }
            };

            var dtoList = new List<RegionDto>
            {
                new() { Id = domainRegions[0].Id, Code = "AKL", Name = "Auckland" },
                new() { Id = domainRegions[1].Id, Code = "WLG", Name = "Wellington" }
            };

            _mockRegionRepository.Setup(r => r.GetAllRegionsAsync(pageNumber, pageSize))
                .ReturnsAsync((domainRegions, totalCount));

            _mockMapper.Setup(m => m.Map<List<RegionDto>>(domainRegions))
                .Returns(dtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Regions retrieved successfully");

            var pagedResponse = result.Data as PagedResponse<RegionDto>;
            pagedResponse.Should().NotBeNull();
            pagedResponse!.Data.Should().HaveCount(2);
            pagedResponse.PageNumber.Should().Be(1);
            pagedResponse.PageSize.Should().Be(2);
            pagedResponse.TotalRecords.Should().Be(2);

            _mockRegionRepository.Verify(r => r.GetAllRegionsAsync(pageNumber, pageSize), Times.Once);
        }
    }
}
