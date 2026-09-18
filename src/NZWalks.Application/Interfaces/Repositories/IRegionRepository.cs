using NZWalks.Domain.Entities;

namespace NZWalks.Application.Interfaces.Repositories
{
    public interface IRegionRepository
    {
        Task<(List<Region> Regions, int TotalCount)> GetAllRegionsAsync(int pageNumber = 1, int pageSize = 10);

        Task<Region?> GetRegionByIdAsync(Guid id);

        Task<Region> CreateRegionAsync(Region region, CancellationToken cancellationToken);

        Task<Region?> UpdateRegionAsync(Guid id, Region region);

        Task<Region?> DeleteRegionAsync(Guid id);
    }
}
