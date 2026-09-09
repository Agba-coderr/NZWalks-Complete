using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NZWalks.Application.Mappings;
using NZWalks.Domain.Entities;
using NZWalks.Application.DTOs;
using NZWalks.Domain.Enums;
using Xunit;

namespace NZWalks.API.Tests.Mappings
{
    public class AutoMapperProfilesTests
    {
        private readonly IMapper _mapper;

        public AutoMapperProfilesTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfiles>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Map_RegionToRegionDto_MapsAllPropertiesCorrectly()
        {
            // Arrange
            var region = new Region
            {
                Id = Guid.NewGuid(),
                Code = "WLG",
                Name = "Wellington",
                RegionImageUrl = "https://example.com/wlg.jpg"
            };

            // Act
            var dto = _mapper.Map<RegionDto>(region);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(region.Id);
            dto.Code.Should().Be(region.Code);
            dto.Name.Should().Be(region.Name);
            dto.RegionImageUrl.Should().Be(region.RegionImageUrl);
        }

        [Fact]
        public void Map_AddRegionRequestDtoToRegion_MapsCorrectly()
        {
            // Arrange
            var addDto = new AddRegionRequestDto
            {
                Code = "BOP",
                Name = "Bay of Plenty",
                RegionImageUrl = "https://example.com/bop.jpg"
            };

            // Act
            var region = _mapper.Map<Region>(addDto);

            // Assert
            region.Should().NotBeNull();
            region.Code.Should().Be(addDto.Code);
            region.Name.Should().Be(addDto.Name);
            region.RegionImageUrl.Should().Be(addDto.RegionImageUrl);
        }

        [Fact]
        public void Map_UpdateRegionDtoToRegion_MapsCorrectly()
        {
            // Arrange
            var updateDto = new UpdateRegionDto
            {
                Code = "CHC",
                Name = "Christchurch",
                RegionImageUrl = "https://example.com/chc.jpg"
            };

            // Act
            var region = _mapper.Map<Region>(updateDto);

            // Assert
            region.Should().NotBeNull();
            region.Code.Should().Be(updateDto.Code);
            region.Name.Should().Be(updateDto.Name);
            region.RegionImageUrl.Should().Be(updateDto.RegionImageUrl);
        }

        [Fact]
        public void Map_WalkToWalkDto_MapsAllPropertiesCorrectly()
        {
            // Arrange
            var region = new Region
            {
                Id = Guid.NewGuid(),
                Code = "AKL",
                Name = "Auckland"
            };

            var walk = new Walk
            {
                Id = Guid.NewGuid(),
                Name = "Milford Track",
                Description = "A world-famous alpine track",
                LengthInKm = 53.5,
                WalkImageUrl = "https://example.com/milford.jpg",
                DifficultyType = DifficultyType.Hard,
                RegionId = region.Id,
                Region = region,
                CreatedByUserId = "user-1"
            };

            // Act
            var dto = _mapper.Map<WalkDto>(walk);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(walk.Id);
            dto.Name.Should().Be(walk.Name);
            dto.Description.Should().Be(walk.Description);
            dto.LengthInKm.Should().Be(walk.LengthInKm);
            dto.DifficultyType.Should().Be(walk.DifficultyType);
            dto.RegionId.Should().Be(walk.RegionId);
            dto.Region.Should().NotBeNull();
            dto.Region.Name.Should().Be(region.Name);
        }

        [Fact]
        public void Map_AddWalkRequestDtoToWalk_MapsCorrectly()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var addDto = new AddWalkRequestDto
            {
                Name = "Routeburn Track",
                Description = "Alpine scenery",
                LengthInKm = 32.0,
                WalkImageUrl = "https://example.com/routeburn.jpg",
                DifficultyType = DifficultyType.Moderate,
                RegionId = regionId
            };

            // Act
            var walk = _mapper.Map<Walk>(addDto);

            // Assert
            walk.Should().NotBeNull();
            walk.Name.Should().Be(addDto.Name);
            walk.Description.Should().Be(addDto.Description);
            walk.LengthInKm.Should().Be(addDto.LengthInKm);
            walk.DifficultyType.Should().Be(addDto.DifficultyType);
            walk.RegionId.Should().Be(regionId);
        }

        [Fact]
        public void Map_ImageUploadRequestDtoToImage_MapsComputedProperties()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("landscape.PNG");
            mockFile.Setup(f => f.Length).Returns(2048);

            var stream = new MemoryStream(new byte[2048]);

            var uploadDto = new ImageUploadRequestDto
            {
                FileStream = stream,
                FileName = "My Landscape Photo.PNG",
                FileDescription = "Description of photo"
            };

            // Act
            var image = _mapper.Map<Image>(uploadDto);

            // Assert
            image.Should().NotBeNull();
            image.FileName.Should().Be("My Landscape Photo.PNG");
            image.FileDescription.Should().Be("Description of photo");
            image.FileExtension.Should().Be(".PNG");
            image.FileSizeInBytes.Should().Be(2048);
        }
    }
}
