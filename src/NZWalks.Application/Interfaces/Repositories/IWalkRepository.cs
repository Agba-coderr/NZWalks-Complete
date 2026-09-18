using NZWalks.Domain.Entities;
using NZWalks.Application.DTOs;
using NZWalks.Domain.Enums;

namespace NZWalks.Application.Interfaces.Repositories
{
    public interface IWalkRepository
    {
        Task<(List<Walk> Walks, int TotalCount)> GetAllWalksAsync(string? filterOn = null, string? filterQuery = null, int pageNumber = 1, int pageSize = 10);

        Task<(List<Walk> Walks, int TotalCount)> GetWalksByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10);

        Task<(List<Walk> Walks, int TotalCount)> GetWalksByRegionIdAsync(Guid regionId, int pageNumber = 1, int pageSize = 10);

        Task<(List<Walk> Walks, int TotalCount)> GetWalksByDifficultyAsync(DifficultyType difficulty, int pageNumber = 1, int pageSize = 10);

        Task<Walk?> GetLongestWalkByUserIdAsync(string userId);

        Task<Walk?> GetWalkByIdAsync(Guid id);

        Task<Walk> CreateWalkAsync(Walk walk);

        Task<Walk?> UpdateWalkAsync(Guid id, Walk walk);

        Task<Walk?> DeleteWalkAsync(Guid id);
    }
}
