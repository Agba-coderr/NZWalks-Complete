using NZWalks.Application.Common;
using NZWalks.Application.DTOs;

namespace NZWalks.Application.Interfaces.Services
{
    public interface IRegionService
    {
        Task<Result> GetAllRegionsAsync(int pageNumber = 1, int pageSize = 10);

        Task<Result> GetRegionByIdAsync(Guid id);

        Task<Result> CreateRegionAsync(AddRegionRequestDto addRegionRequestDto);

        Task<Result> UpdateRegionAsync(Guid id, UpdateRegionDto updateRegionDto);

        Task<Result> DeleteRegionAsync(Guid id);
    }
}

