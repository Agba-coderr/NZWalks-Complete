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
    public class GetAllWalksQueryHandlerTests
    {
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetAllWalksQueryHandler _handler;

        public GetAllWalksQueryHandlerTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetAllWalksQueryHandler(_mockWalkRepository.Object, _mockMapper.Object);
        }

        [Theory]
        [InlineData(0, 10, "Page number must be greater than 0.")]
        [InlineData(1, 0, "Page size must be greater than 0.")]
        [InlineData(1, 51, "Page size cannot exceed 50.")]
        public async Task Handle_WhenInvalidPagination_ReturnsBadRequestWithoutCallingRepo(
            int pageNumber, int pageSize, string expectedErrorMessage)
        {
            // Arrange
            var query = new GetAllWalksQuery(FilterOn: null, FilterQuery: null, PageNumber: pageNumber, PageSize: pageSize);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be(expectedErrorMessage);

            _mockWalkRepository.Verify(
                r => r.GetAllWalksAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenValidPagination_ReturnsPagedSuccessResult()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            int totalCount = 2;
            var query = new GetAllWalksQuery(FilterOn: "Name", FilterQuery: "Track", PageNumber: pageNumber, PageSize: pageSize);

            var walks = new List<Walk>
            {
                TestDataHelper.CreateWalk(name: "Track 1"),
                TestDataHelper.CreateWalk(name: "Track 2")
            };

            var walkDtos = new List<WalkDto>
            {
                TestDataHelper.CreateWalkDto(walks[0].Id, name: "Track 1"),
                TestDataHelper.CreateWalkDto(walks[1].Id, name: "Track 2")
            };

            _mockWalkRepository.Setup(r => r.GetAllWalksAsync("Name", "Track", pageNumber, pageSize))
                .ReturnsAsync((walks, totalCount));

            _mockMapper.Setup(m => m.Map<List<WalkDto>>(walks))
                .Returns(walkDtos);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Walks retrieved successfully");

            var pagedResponse = result.Data as PagedResponse<WalkDto>;
            pagedResponse.Should().NotBeNull();
            pagedResponse!.Data.Should().HaveCount(2);
            pagedResponse.PageNumber.Should().Be(1);
            pagedResponse.PageSize.Should().Be(2);
            pagedResponse.TotalRecords.Should().Be(2);

            _mockWalkRepository.Verify(r => r.GetAllWalksAsync("Name", "Track", pageNumber, pageSize), Times.Once);
        }
    }
}
