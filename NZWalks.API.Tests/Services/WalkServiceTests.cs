using AutoMapper;
using FluentAssertions;
using Moq;
using NZWalks.Application.Common;
using NZWalks.Domain.Entities;
using NZWalks.Application.DTOs;
using NZWalks.Domain.Enums;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using NZWalks.Application.Services;
using Xunit;

namespace NZWalks.API.Tests.Services
{
    public class WalkServiceTests
    {
        // Mocks for dependencies
        private readonly Mock<IWalkRepository> _mockWalkRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IRegionRepository> _mockRegionRepository;

        // System Under Test (SUT)
        private readonly WalkService _walkService;

        public WalkServiceTests()
        {
            _mockWalkRepository = new Mock<IWalkRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockRegionRepository = new Mock<IRegionRepository>();

            _walkService = new WalkService(
                _mockWalkRepository.Object,
                _mockMapper.Object,
                _mockRegionRepository.Object);
        }

        #region Helper Methods

        private Region CreateSampleRegion(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Code = "AKL",
            Name = "Auckland",
            RegionImageUrl = "https://example.com/region.jpg"
        };

        private RegionDto CreateSampleRegionDto(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Code = "AKL",
            Name = "Auckland",
            RegionImageUrl = "https://example.com/region.jpg"
        };

        private Walk CreateSampleWalk(Guid? id = null, Guid? regionId = null, string userId = "user-123")
        {
            var rId = regionId ?? Guid.NewGuid();
            var region = CreateSampleRegion(rId);
            return new Walk
            {
                Id = id ?? Guid.NewGuid(),
                Name = "Coast to Coast",
                Description = "A scenic walk across Auckland",
                LengthInKm = 16.0,
                WalkImageUrl = "https://example.com/walk.jpg",
                DifficultyType = DifficultyType.Moderate,
                RegionId = rId,
                Region = region,
                CreatedByUserId = userId
            };
        }

        private WalkDto CreateSampleWalkDto(Guid? id = null, Guid? regionId = null)
        {
            var rId = regionId ?? Guid.NewGuid();
            return new WalkDto
            {
                Id = id ?? Guid.NewGuid(),
                Name = "Coast to Coast",
                Description = "A scenic walk across Auckland",
                LengthInKm = 16.0,
                WalkImageUrl = "https://example.com/walk.jpg",
                DifficultyType = DifficultyType.Moderate,
                RegionId = rId,
                Region = CreateSampleRegionDto(rId)
            };
        }

        #endregion

        #region CreateWalkAsync Tests

        [Fact]
        public async Task CreateWalkAsync_WhenRegionDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var addWalkDto = new AddWalkRequestDto
            {
                Name = "Mountain Track",
                Description = "Beautiful mountain climb",
                LengthInKm = 12.5,
                DifficultyType = DifficultyType.Hard,
                RegionId = regionId
            };

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync((Region?)null);

            // Act
            var result = await _walkService.CreateWalkAsync(addWalkDto, "user-123");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Region ID {regionId} does not exist");
            Assert.Null(result.Data);

            _mockWalkRepository.Verify(r => r.CreateWalkAsync(It.IsAny<Walk>()), Times.Never);
        }

        [Fact]
        public async Task CreateWalkAsync_WhenRegionExists_ReturnsSuccessResultWithCreatedWalk()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var region = CreateSampleRegion(regionId);
            var userId = "user-123";

            var addWalkDto = new AddWalkRequestDto
            {
                Name = "Mountain Track",
                Description = "Beautiful mountain climb",
                LengthInKm = 12.5,
                WalkImageUrl = "https://example.com/walk.jpg",
                DifficultyType = DifficultyType.Hard,
                RegionId = regionId
            };

            var createdWalk = new Walk
            {
                Id = Guid.NewGuid(),
                Name = addWalkDto.Name,
                Description = addWalkDto.Description,
                LengthInKm = addWalkDto.LengthInKm,
                WalkImageUrl = addWalkDto.WalkImageUrl,
                DifficultyType = addWalkDto.DifficultyType,
                RegionId = regionId,
                Region = region,
                CreatedByUserId = userId
            };

            var expectedWalkDto = CreateSampleWalkDto(createdWalk.Id, regionId);

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync(region);

            _mockWalkRepository.Setup(r => r.CreateWalkAsync(It.IsAny<Walk>()))
                .ReturnsAsync(createdWalk);

            _mockMapper.Setup(m => m.Map<WalkDto>(createdWalk))
                .Returns(expectedWalkDto);

            // Act
            var result = await _walkService.CreateWalkAsync(addWalkDto, userId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Walk created successfully.");

            var actualData = result.Data as WalkDto;
            actualData.Should().NotBeNull();
            actualData.Should().BeEquivalentTo(expectedWalkDto);

            _mockWalkRepository.Verify(r => r.CreateWalkAsync(It.Is<Walk>(w =>
                w.Name == addWalkDto.Name &&
                w.CreatedByUserId == userId &&
                w.RegionId == regionId)), Times.Once);
        }

        #endregion

        #region DeleteWalkAsync Tests

        [Fact]
        public async Task DeleteWalkAsync_WhenWalkDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync((Walk?)null);

            // Act
            var result = await _walkService.DeleteWalkAsync(walkId, "user-123", isAdmin: false);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("This walk does not exist.");
            Assert.Null(result.Data);

            _mockWalkRepository.Verify(r => r.DeleteWalkAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteWalkAsync_WhenUserIsNotOwnerAndNotAdmin_ReturnsForbiddenResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var existingWalk = CreateSampleWalk(walkId, userId: "owner-user-id");

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            // Act
            var result = await _walkService.DeleteWalkAsync(walkId, "different-user-id", isAdmin: false);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(403);
            result.Message.Should().Be("You do not own this walk.");
            Assert.Null(result.Data);

            _mockWalkRepository.Verify(r => r.DeleteWalkAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteWalkAsync_WhenUserIsOwner_DeletesSuccessfully()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-user-id";
            var existingWalk = CreateSampleWalk(walkId, userId: userId);
            var expectedDto = CreateSampleWalkDto(walkId);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockWalkRepository.Setup(r => r.DeleteWalkAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockMapper.Setup(m => m.Map<WalkDto>(existingWalk))
                .Returns(expectedDto);

            // Act
            var result = await _walkService.DeleteWalkAsync(walkId, userId, isAdmin: false);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Walk deleted successfully");

            var actualData = result.Data as WalkDto;
            actualData.Should().NotBeNull();
            actualData.Should().BeEquivalentTo(expectedDto);

            _mockWalkRepository.Verify(r => r.DeleteWalkAsync(walkId), Times.Once);
        }

        [Fact]
        public async Task DeleteWalkAsync_WhenUserIsAdminEvenIfNotOwner_DeletesSuccessfully()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var existingWalk = CreateSampleWalk(walkId, userId: "creator-user-id");
            var expectedDto = CreateSampleWalkDto(walkId);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockWalkRepository.Setup(r => r.DeleteWalkAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockMapper.Setup(m => m.Map<WalkDto>(existingWalk))
                .Returns(expectedDto);

            // Act: Admin deletes walk owned by someone else
            var result = await _walkService.DeleteWalkAsync(walkId, "admin-user-id", isAdmin: true);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Walk deleted successfully");

            _mockWalkRepository.Verify(r => r.DeleteWalkAsync(walkId), Times.Once);
        }

        [Fact]
        public async Task DeleteWalkAsync_WhenDeleteFailsInRepository_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-user-id";
            var existingWalk = CreateSampleWalk(walkId, userId: userId);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockWalkRepository.Setup(r => r.DeleteWalkAsync(walkId))
                .ReturnsAsync((Walk?)null);

            // Act
            var result = await _walkService.DeleteWalkAsync(walkId, userId, isAdmin: false);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("This walk no longer exists.");
        }

        #endregion

        #region GetAllWalksAsync Tests

        [Fact]
        public async Task GetAllWalksAsync_WhenValidPagination_ReturnsPagedSuccessResult()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            int totalCount = 2;
            var walks = new List<Walk> { CreateSampleWalk(), CreateSampleWalk() };
            var walkDtos = new List<WalkDto> { CreateSampleWalkDto(walks[0].Id), CreateSampleWalkDto(walks[1].Id) };

            _mockWalkRepository.Setup(r => r.GetAllWalksAsync(null, null, pageNumber, pageSize))
                .ReturnsAsync((walks, totalCount));

            _mockMapper.Setup(m => m.Map<List<WalkDto>>(walks))
                .Returns(walkDtos);

            // Act
            var result = await _walkService.GetAllWalksAsync(null, null, pageNumber, pageSize);

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

            _mockWalkRepository.Verify(r => r.GetAllWalksAsync(null, null, pageNumber, pageSize), Times.Once);
        }

        [Theory]
        [InlineData(0, 10, "Page number must be greater than 0.")]
        [InlineData(1, 0, "Page size must be greater than 0.")]
        [InlineData(1, 51, "Page size cannot exceed 50.")]
        public async Task GetAllWalksAsync_WhenInvalidPagination_ReturnsBadRequestWithoutCallingRepo(
            int pageNumber, int pageSize, string expectedErrorMessage)
        {
            // Act
            var result = await _walkService.GetAllWalksAsync(null, null, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be(expectedErrorMessage);

            _mockWalkRepository.Verify(
                r => r.GetAllWalksAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        #endregion

        #region GetLongestWalkByUserIdAsync Tests

        [Fact]
        public async Task GetLongestWalkByUserIdAsync_WhenWalkExists_ReturnsSuccessResult()
        {
            // Arrange
            var userId = "user-456";
            var longestWalk = CreateSampleWalk(userId: userId);
            var expectedDto = CreateSampleWalkDto(longestWalk.Id);

            _mockWalkRepository.Setup(r => r.GetLongestWalkByUserIdAsync(userId))
                .ReturnsAsync(longestWalk);

            _mockMapper.Setup(m => m.Map<WalkDto>(longestWalk))
                .Returns(expectedDto);

            // Act
            var result = await _walkService.GetLongestWalkByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be($"Longest walk for user: {userId} retrieved successfully");

            var actualData = result.Data as WalkDto;
            actualData.Should().NotBeNull();
            actualData.Should().BeEquivalentTo(expectedDto);

            _mockWalkRepository.Verify(r => r.GetLongestWalkByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetLongestWalkByUserIdAsync_WhenWalkDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var userId = "unknown-user";

            _mockWalkRepository.Setup(r => r.GetLongestWalkByUserIdAsync(userId))
                .ReturnsAsync((Walk?)null);

            // Act
            var result = await _walkService.GetLongestWalkByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Longest walk not found for user: {userId}");
            Assert.Null(result.Data);
        }

        #endregion

        #region GetWalkByIdAsync Tests

        [Fact]
        public async Task GetWalkByIdAsync_WhenWalkExists_ReturnsSuccessResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var walk = CreateSampleWalk(walkId);
            var expectedDto = CreateSampleWalkDto(walkId);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(walk);

            _mockMapper.Setup(m => m.Map<WalkDto>(walk))
                .Returns(expectedDto);

            // Act
            var result = await _walkService.GetWalkByIdAsync(walkId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be($"Walk with ID {walkId} retrieved successfully");

            var actualData = result.Data as WalkDto;
            actualData.Should().NotBeNull();
            actualData.Should().BeEquivalentTo(expectedDto);

            _mockWalkRepository.Verify(r => r.GetWalkByIdAsync(walkId), Times.Once);
        }

        [Fact]
        public async Task GetWalkByIdAsync_WhenWalkDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync((Walk?)null);

            // Act
            var result = await _walkService.GetWalkByIdAsync(walkId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Walk with ID {walkId} not found.");
            Assert.Null(result.Data);

            _mockMapper.Verify(m => m.Map<WalkDto>(It.IsAny<Walk>()), Times.Never);
        }

        #endregion

        #region GetWalksByDifficultyAsync Tests

        [Fact]
        public async Task GetWalksByDifficultyAsync_WhenValidPagination_ReturnsPagedSuccessResult()
        {
            // Arrange
            var difficulty = DifficultyType.Easy;
            int pageNumber = 1;
            int pageSize = 10;
            var walks = new List<Walk> { CreateSampleWalk() };
            var dtoList = new List<WalkDto> { CreateSampleWalkDto(walks[0].Id) };

            _mockWalkRepository.Setup(r => r.GetWalksByDifficultyAsync(difficulty, pageNumber, pageSize))
                .ReturnsAsync((walks, 1));

            _mockMapper.Setup(m => m.Map<List<WalkDto>>(walks))
                .Returns(dtoList);

            // Act
            var result = await _walkService.GetWalksByDifficultyAsync(difficulty, pageNumber, pageSize);

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

        [Fact]
        public async Task GetWalksByDifficultyAsync_WhenInvalidPagination_ReturnsBadRequest()
        {
            // Act
            var result = await _walkService.GetWalksByDifficultyAsync(DifficultyType.Easy, pageNumber: 0, pageSize: 10);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            _mockWalkRepository.Verify(
                r => r.GetWalksByDifficultyAsync(It.IsAny<DifficultyType>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        #endregion

        #region GetWalksByRegionIdAsync Tests

        [Fact]
        public async Task GetWalksByRegionIdAsync_WhenRegionDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync((Region?)null);

            // Act
            var result = await _walkService.GetWalksByRegionIdAsync(regionId, 1, 10);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Region with ID {regionId} does not exist.");
            Assert.Null(result.Data);

            _mockWalkRepository.Verify(
                r => r.GetWalksByRegionIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task GetWalksByRegionIdAsync_WhenRegionExists_ReturnsPagedSuccessResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var region = CreateSampleRegion(regionId);
            var walks = new List<Walk> { CreateSampleWalk(regionId: regionId) };
            var dtoList = new List<WalkDto> { CreateSampleWalkDto(walks[0].Id, regionId) };

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync(region);

            _mockWalkRepository.Setup(r => r.GetWalksByRegionIdAsync(regionId, 1, 10))
                .ReturnsAsync((walks, 1));

            _mockMapper.Setup(m => m.Map<List<WalkDto>>(walks))
                .Returns(dtoList);

            // Act
            var result = await _walkService.GetWalksByRegionIdAsync(regionId, 1, 10);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be($"Walks for the region: {regionId} retrieved successfully");

            var pagedResponse = result.Data as PagedResponse<WalkDto>;
            pagedResponse.Should().NotBeNull();
            pagedResponse!.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetWalksByRegionIdAsync_WhenInvalidPagination_ReturnsBadRequest()
        {
            // Act
            var result = await _walkService.GetWalksByRegionIdAsync(Guid.NewGuid(), pageNumber: -1, pageSize: 10);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            _mockRegionRepository.Verify(r => r.GetRegionByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region GetWalksByUserIdAsync Tests

        [Fact]
        public async Task GetWalksByUserIdAsync_WhenValidPagination_ReturnsPagedSuccessResult()
        {
            // Arrange
            var userId = "user-999";
            var walks = new List<Walk> { CreateSampleWalk(userId: userId) };
            var dtoList = new List<WalkDto> { CreateSampleWalkDto(walks[0].Id) };

            _mockWalkRepository.Setup(r => r.GetWalksByUserIdAsync(userId, 1, 10))
                .ReturnsAsync((walks, 1));

            _mockMapper.Setup(m => m.Map<List<WalkDto>>(walks))
                .Returns(dtoList);

            // Act
            var result = await _walkService.GetWalksByUserIdAsync(userId, 1, 10);

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

        [Fact]
        public async Task GetWalksByUserIdAsync_WhenInvalidPagination_ReturnsBadRequest()
        {
            // Act
            var result = await _walkService.GetWalksByUserIdAsync("user-999", pageNumber: 1, pageSize: 55);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            _mockWalkRepository.Verify(
                r => r.GetWalksByUserIdAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        #endregion

        #region UpdateWalkAsync Tests

        [Fact]
        public async Task UpdateWalkAsync_WhenWalkDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var updateDto = new UpdateWalkDto
            {
                Name = "Updated Walk",
                Description = "Updated description",
                LengthInKm = 5.0,
                DifficultyType = DifficultyType.Easy,
                RegionId = Guid.NewGuid()
            };

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync((Walk?)null);

            // Act
            var result = await _walkService.UpdateWalkAsync(walkId, updateDto, "user-123", isAdmin: false);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("This walk does not exist.");
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task UpdateWalkAsync_WhenUserIsNotOwner_ReturnsForbiddenResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var existingWalk = CreateSampleWalk(walkId, userId: "creator-user-id");
            var updateDto = new UpdateWalkDto
            {
                Name = "Updated Walk",
                Description = "Updated description",
                LengthInKm = 5.0,
                DifficultyType = DifficultyType.Easy,
                RegionId = Guid.NewGuid()
            };

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            // Act: Different user tries to update
            var result = await _walkService.UpdateWalkAsync(walkId, updateDto, "other-user-id", isAdmin: false);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(403);
            result.Message.Should().Be("You do not own this walk.");
            Assert.Null(result.Data);

            _mockRegionRepository.Verify(r => r.GetRegionByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockWalkRepository.Verify(r => r.UpdateWalkAsync(It.IsAny<Guid>(), It.IsAny<Walk>()), Times.Never);
        }

        [Fact]
        public async Task UpdateWalkAsync_WhenRegionDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-user-id";
            var regionId = Guid.NewGuid();
            var existingWalk = CreateSampleWalk(walkId, userId: userId);

            var updateDto = new UpdateWalkDto
            {
                Name = "Updated Walk",
                Description = "Updated description",
                LengthInKm = 5.0,
                DifficultyType = DifficultyType.Easy,
                RegionId = regionId
            };

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync((Region?)null);

            // Act
            var result = await _walkService.UpdateWalkAsync(walkId, updateDto, userId, isAdmin: false);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("Invalid region ID.");
            Assert.Null(result.Data);

            _mockWalkRepository.Verify(r => r.UpdateWalkAsync(It.IsAny<Guid>(), It.IsAny<Walk>()), Times.Never);
        }

        [Fact]
        public async Task UpdateWalkAsync_WhenRepositoryUpdateFails_ReturnsNotFoundResult()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-user-id";
            var regionId = Guid.NewGuid();
            var region = CreateSampleRegion(regionId);
            var existingWalk = CreateSampleWalk(walkId, userId: userId);

            var updateDto = new UpdateWalkDto
            {
                Name = "Updated Walk",
                Description = "Updated description",
                LengthInKm = 5.0,
                DifficultyType = DifficultyType.Easy,
                RegionId = regionId
            };

            var mappedWalkDomain = new Walk
            {
                Name = updateDto.Name,
                Description = updateDto.Description,
                LengthInKm = updateDto.LengthInKm,
                DifficultyType = updateDto.DifficultyType,
                RegionId = regionId,
                Region = region,
                CreatedByUserId = userId
            };

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync(region);

            _mockMapper.Setup(m => m.Map<Walk>(updateDto))
                .Returns(mappedWalkDomain);

            _mockWalkRepository.Setup(r => r.UpdateWalkAsync(walkId, It.IsAny<Walk>()))
                .ReturnsAsync((Walk?)null);

            // Act
            var result = await _walkService.UpdateWalkAsync(walkId, updateDto, userId, isAdmin: false);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Walk with ID {walkId} not found.");
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task UpdateWalkAsync_WhenValidInput_ReturnsSuccessResultWithUpdatedWalk()
        {
            // Arrange
            var walkId = Guid.NewGuid();
            var userId = "owner-user-id";
            var regionId = Guid.NewGuid();
            var region = CreateSampleRegion(regionId);
            var existingWalk = CreateSampleWalk(walkId, userId: userId);

            var updateDto = new UpdateWalkDto
            {
                Name = "Updated Walk",
                Description = "Updated description",
                LengthInKm = 8.5,
                DifficultyType = DifficultyType.Moderate,
                RegionId = regionId
            };

            var mappedWalkDomain = new Walk
            {
                Name = updateDto.Name,
                Description = updateDto.Description,
                LengthInKm = updateDto.LengthInKm,
                DifficultyType = updateDto.DifficultyType,
                RegionId = regionId,
                Region = region,
                CreatedByUserId = userId
            };

            var updatedWalk = new Walk
            {
                Id = walkId,
                Name = updateDto.Name,
                Description = updateDto.Description,
                LengthInKm = updateDto.LengthInKm,
                DifficultyType = updateDto.DifficultyType,
                RegionId = regionId,
                Region = region,
                CreatedByUserId = userId
            };

            var expectedDto = CreateSampleWalkDto(walkId, regionId);

            _mockWalkRepository.Setup(r => r.GetWalkByIdAsync(walkId))
                .ReturnsAsync(existingWalk);

            _mockRegionRepository.Setup(r => r.GetRegionByIdAsync(regionId))
                .ReturnsAsync(region);

            _mockMapper.Setup(m => m.Map<Walk>(updateDto))
                .Returns(mappedWalkDomain);

            _mockWalkRepository.Setup(r => r.UpdateWalkAsync(walkId, It.IsAny<Walk>()))
                .ReturnsAsync(updatedWalk);

            _mockMapper.Setup(m => m.Map<WalkDto>(updatedWalk))
                .Returns(expectedDto);

            // Act
            var result = await _walkService.UpdateWalkAsync(walkId, updateDto, userId, isAdmin: false);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Walk updated successfully");

            var actualData = result.Data as WalkDto;
            actualData.Should().NotBeNull();
            actualData.Should().BeEquivalentTo(expectedDto);

            _mockWalkRepository.Verify(r => r.UpdateWalkAsync(walkId, It.Is<Walk>(w =>
                w.Region == region &&
                w.CreatedByUserId == userId)), Times.Once);
        }

        #endregion
    }
}
