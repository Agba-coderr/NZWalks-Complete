using NZWalks.Application.DTOs;
using NZWalks.Domain.Entities;
using NZWalks.Domain.Enums;

namespace NZWalks.API.Tests.Common
{
    public static class TestDataHelper
    {
        public static Region CreateRegion(Guid? id = null, string code = "AKL", string name = "Auckland", string? imageUrl = null)
        {
            return new Region
            {
                Id = id ?? Guid.NewGuid(),
                Code = code,
                Name = name,
                RegionImageUrl = imageUrl
            };
        }

        public static RegionDto CreateRegionDto(Guid? id = null, string code = "AKL", string name = "Auckland", string? imageUrl = null)
        {
            return new RegionDto
            {
                Id = id ?? Guid.NewGuid(),
                Code = code,
                Name = name,
                RegionImageUrl = imageUrl
            };
        }

        public static Walk CreateWalk(
            Guid? id = null,
            string name = "Coast to Coast",
            string description = "Scenic walk",
            double lengthInKm = 10.0,
            DifficultyType difficulty = DifficultyType.Moderate,
            Guid? regionId = null,
            Region? region = null,
            string userId = "user-123",
            string? imageUrl = null)
        {
            var rId = regionId ?? region?.Id ?? Guid.NewGuid();
            var reg = region ?? CreateRegion(rId);
            return new Walk
            {
                Id = id ?? Guid.NewGuid(),
                Name = name,
                Description = description,
                LengthInKm = lengthInKm,
                DifficultyType = difficulty,
                RegionId = rId,
                Region = reg,
                CreatedByUserId = userId,
                WalkImageUrl = imageUrl
            };
        }

        public static WalkDto CreateWalkDto(
            Guid? id = null,
            string name = "Coast to Coast",
            string description = "Scenic walk",
            double lengthInKm = 10.0,
            DifficultyType difficulty = DifficultyType.Moderate,
            Guid? regionId = null,
            RegionDto? regionDto = null,
            string? imageUrl = null)
        {
            var rId = regionId ?? regionDto?.Id ?? Guid.NewGuid();
            var regDto = regionDto ?? CreateRegionDto(rId);
            return new WalkDto
            {
                Id = id ?? Guid.NewGuid(),
                Name = name,
                Description = description,
                LengthInKm = lengthInKm,
                DifficultyType = difficulty,
                RegionId = rId,
                Region = regDto,
                WalkImageUrl = imageUrl
            };
        }
    }
}
