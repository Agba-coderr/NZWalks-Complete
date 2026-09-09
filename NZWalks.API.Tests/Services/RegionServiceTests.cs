using AutoMapper;
using FluentAssertions;
using Moq;
using NZWalks.Application.Common;
using NZWalks.Domain.Entities;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using NZWalks.Application.Services;
using Xunit;

namespace NZWalks.API.Tests.Services
{
    public class RegionServiceTests
    {
        // 1. Dependencies to be mocked
        private readonly Mock<IRegionRepository> _mockRegionRepository;
        private readonly Mock<IMapper> _mockMapper;

        // 2. System Under Test (SUT)
        private readonly RegionService _regionService;

        public RegionServiceTests()
        {
            // Initializes fresh mocks and a new service instance before EVERY test run
            _mockRegionRepository = new Mock<IRegionRepository>();
            _mockMapper = new Mock<IMapper>();

            _regionService = new RegionService(_mockRegionRepository.Object, _mockMapper.Object);
        }

        #region CreateRegionAsync Tests

        [Fact]
        public async Task CreateRegionAsync_WhenValidInput_ReturnsSuccessResultWithCreatedRegion()
        {
            // Arrange
            var requestDto = new AddRegionRequestDto
            {
                Code = "AKL",
                Name = "Auckland",
                RegionImageUrl = "https://example.com/auckland.jpg"
            };

            var domainModel = new Region
            {
                Id = Guid.NewGuid(),
                Code = "AKL",
                Name = "Auckland",
                RegionImageUrl = "https://example.com/auckland.jpg"
            };

            var expectedResponseDto = new RegionDto
            {
                Id = domainModel.Id,
                Code = domainModel.Code,
                Name = domainModel.Name,
                RegionImageUrl = domainModel.RegionImageUrl
            };

            // Setup mapping: AddRegionRequestDto -> Region domain model
            _mockMapper.Setup(m => m.Map<Region>(requestDto))
                .Returns(domainModel);

            // Setup repository: CreateRegionAsync returns created Region
            _mockRegionRepository.Setup(r => r.CreateRegionAsync(domainModel))
                .ReturnsAsync(domainModel);

            // Setup mapping: Region domain model -> RegionDto
            _mockMapper.Setup(m => m.Map<RegionDto>(domainModel))
                .Returns(expectedResponseDto);

            // Act
            var result = await _regionService.CreateRegionAsync(requestDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Region created successfully");

            var actualData = result.Data as RegionDto;
            actualData.Should().NotBeNull();
            actualData.Should().BeEquivalentTo(expectedResponseDto);

            // Verify interactions
            _mockRegionRepository.Verify(r => r.CreateRegionAsync(domainModel), Times.Once);
        }

        #endregion

        #region GetRegionByIdAsync Tests

        [Fact]
        public async Task GetRegionByIdAsync_WhenRegionExists_ReturnsSuccessResultWithRegionDto()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var existingRegion = new Region
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
                .ReturnsAsync(existingRegion);

            _mockMapper.Setup(m => m.Map<RegionDto>(existingRegion))
                .Returns(expectedDto);

            // Act
            var result = await _regionService.GetRegionByIdAsync(regionId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Region retrieved successfully");

            var actualData = result.Data as RegionDto;
            actualData.Should().NotBeNull();
            actualData.Should().BeEquivalentTo(expectedDto);

            _mockRegionRepository.Verify(r => r.GetRegionByIdAsync(regionId), Times.Once);
        }

        [Fact]
        public async Task GetRegionByIdAsync_WhenRegionDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync((Region?)null);

            // Act
            var result = await _regionService.GetRegionByIdAsync(regionId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Region with ID {regionId} was not found");
            Assert.Null(result.Data);

            // Verify mapping was not called since region was null
            _mockMapper.Verify(m => m.Map<RegionDto>(It.IsAny<Region>()), Times.Never);
        }

        #endregion

        #region GetAllRegionsAsync Tests

        [Fact]
        public async Task GetAllRegionsAsync_WhenValidPagination_ReturnsPagedSuccessResult()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            int totalCount = 2;

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
            var result = await _regionService.GetAllRegionsAsync(pageNumber, pageSize);

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
            pagedResponse.TotalPages.Should().Be(1);
            pagedResponse.HasNextPage.Should().BeFalse();
            pagedResponse.HasPreviousPage.Should().BeFalse();

            _mockRegionRepository.Verify(r => r.GetAllRegionsAsync(pageNumber, pageSize), Times.Once);
        }

        [Theory]
        [InlineData(0, 10, "Page number must be greater than 0.")]
        [InlineData(-1, 10, "Page number must be greater than 0.")]
        [InlineData(1, 0, "Page size must be greater than 0.")]
        [InlineData(1, -5, "Page size must be greater than 0.")]
        [InlineData(1, 51, "Page size cannot exceed 50.")]
        public async Task GetAllRegionsAsync_WhenInvalidPagination_ReturnsBadRequestResultWithoutCallingRepository(
            int pageNumber, int pageSize, string expectedErrorMessage)
        {
            // Act
            var result = await _regionService.GetAllRegionsAsync(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be(expectedErrorMessage);
            Assert.Null(result.Data);

            // Repository should NOT be queried when pagination validation fails
            _mockRegionRepository.Verify(
                r => r.GetAllRegionsAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        #endregion

        #region UpdateRegionAsync Tests

        [Fact]
        public async Task UpdateRegionAsync_WhenRegionExists_ReturnsSuccessResultWithUpdatedRegion()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var updateDto = new UpdateRegionDto
            {
                Code = "CHC",
                Name = "Christchurch Updated",
                RegionImageUrl = "https://example.com/updated.jpg"
            };

            var domainModel = new Region
            {
                Code = "CHC",
                Name = "Christchurch Updated",
                RegionImageUrl = "https://example.com/updated.jpg"
            };

            var updatedDomainModel = new Region
            {
                Id = regionId,
                Code = "CHC",
                Name = "Christchurch Updated",
                RegionImageUrl = "https://example.com/updated.jpg"
            };

            var expectedDto = new RegionDto
            {
                Id = regionId,
                Code = "CHC",
                Name = "Christchurch Updated",
                RegionImageUrl = "https://example.com/updated.jpg"
            };

            _mockMapper.Setup(m => m.Map<Region>(updateDto))
                .Returns(domainModel);

            _mockRegionRepository.Setup(r => r.UpdateRegionAsync(regionId, domainModel))
                .ReturnsAsync(updatedDomainModel);

            _mockMapper.Setup(m => m.Map<RegionDto>(updatedDomainModel))
                .Returns(expectedDto);

            // Act
            var result = await _regionService.UpdateRegionAsync(regionId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Region updated successfully");

            var actualData = result.Data as RegionDto;
            actualData.Should().NotBeNull();
            actualData.Should().BeEquivalentTo(expectedDto);

            _mockRegionRepository.Verify(r => r.UpdateRegionAsync(regionId, domainModel), Times.Once);
        }

        [Fact]
        public async Task UpdateRegionAsync_WhenRegionDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var updateDto = new UpdateRegionDto
            {
                Code = "CHC",
                Name = "Christchurch",
                RegionImageUrl = null
            };

            var domainModel = new Region
            {
                Code = "CHC",
                Name = "Christchurch",
                RegionImageUrl = null
            };

            _mockMapper.Setup(m => m.Map<Region>(updateDto))
                .Returns(domainModel);

            _mockRegionRepository.Setup(r => r.UpdateRegionAsync(regionId, domainModel))
                .ReturnsAsync((Region?)null);

            // Act
            var result = await _regionService.UpdateRegionAsync(regionId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Region with ID {regionId} was not found");
            Assert.Null(result.Data);

            _mockMapper.Verify(m => m.Map<RegionDto>(It.IsAny<Region>()), Times.Never);
        }

        #endregion

        #region DeleteRegionAsync Tests

        [Fact]
        public async Task DeleteRegionAsync_WhenRegionExists_ReturnsSuccessResultWithDeletedRegion()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var existingRegion = new Region
            {
                Id = regionId,
                Code = "BOP",
                Name = "Bay of Plenty"
            };

            var expectedDto = new RegionDto
            {
                Id = regionId,
                Code = "BOP",
                Name = "Bay of Plenty"
            };

            _mockRegionRepository.Setup(r => r.DeleteRegionAsync(regionId))
                .ReturnsAsync(existingRegion);

            _mockMapper.Setup(m => m.Map<RegionDto>(existingRegion))
                .Returns(expectedDto);

            // Act
            var result = await _regionService.DeleteRegionAsync(regionId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Region deleted successfully");

            var actualData = result.Data as RegionDto;
            actualData.Should().NotBeNull();
            actualData.Should().BeEquivalentTo(expectedDto);

            _mockRegionRepository.Verify(r => r.DeleteRegionAsync(regionId), Times.Once);
        }

        [Fact]
        public async Task DeleteRegionAsync_WhenRegionDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();

            _mockRegionRepository.Setup(r => r.DeleteRegionAsync(regionId))
                .ReturnsAsync((Region?)null);

            // Act
            var result = await _regionService.DeleteRegionAsync(regionId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Region with ID {regionId} was not found");
            Assert.Null(result.Data);

            _mockMapper.Verify(m => m.Map<RegionDto>(It.IsAny<Region>()), Times.Never);
        }

        #endregion
    }
}
