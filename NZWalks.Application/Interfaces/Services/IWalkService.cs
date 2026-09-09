using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Domain.Enums;

namespace NZWalks.Application.Interfaces.Services
{
    public interface IWalkService
    {
        Task<Result> GetAllWalksAsync(string? filterOn = null, string? filterQuery = null, int pageNumber = 1, int pageSize = 10);

        Task<Result> GetWalksByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10);

        Task<Result> GetWalksByRegionIdAsync(Guid regionId, int pageNumber = 1, int pageSize = 10);

        Task<Result> GetWalksByDifficultyAsync(DifficultyType difficulty, int pageNumber = 1, int pageSize = 10);

        Task<Result> GetLongestWalkByUserIdAsync(string userId);

        Task<Result> GetWalkByIdAsync(Guid id);

        Task<Result> CreateWalkAsync(AddWalkRequestDto addWalkRequestDto, string createdByUserId);

        Task<Result> UpdateWalkAsync(Guid id, UpdateWalkDto updateWalkDto, string currentUserId, bool isAdmin);

        Task<Result> DeleteWalkAsync(Guid id, string currentUserId, bool isAdmin);
    }
}
