using NZWalks.Infrastructure.Data;
using NZWalks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NZWalks.Application.Extensions;
using NZWalks.Application.Interfaces.Repositories;

namespace NZWalks.Infrastructure.Repositories
{
    public class SQLRegionRepository : IRegionRepository
    {
        private readonly NZWalksDbContext _dbContext;

        public SQLRegionRepository(NZWalksDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Region> CreateRegionAsync(Region region, CancellationToken cancellationToken)
        {
            await _dbContext.Regions.AddAsync(region, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return region;
        }

        public async Task<Region?> DeleteRegionAsync(Guid id)
        {
            var existingRegion = await _dbContext.Regions.FindAsync(id);
            if (existingRegion == null)
            {
                return null;
            }

            _dbContext.Regions.Remove(existingRegion);
            await _dbContext.SaveChangesAsync();
            return existingRegion;
        }

        public async Task<(List<Region> Regions, int TotalCount)> GetAllRegionsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var regions = _dbContext.Regions.AsQueryable();

            var totalCount = await regions.CountAsync();

            var regionsList = await regions.Paginate(pageNumber, pageSize).ToListAsync();

            return (regionsList, totalCount);
        }

        public async Task<Region?> GetRegionByIdAsync(Guid id)
        {
            return await _dbContext.Regions.FindAsync(id);
        }

        public async Task<Region?> UpdateRegionAsync(Guid id, Region region)
        {
            var existingRegion = await _dbContext.Regions.FindAsync(id);
            if (existingRegion == null)
            {
                return null;
            }

            existingRegion.Code = region.Code;
            existingRegion.Name = region.Name;
            existingRegion.RegionImageUrl = region.RegionImageUrl;

            await _dbContext.SaveChangesAsync();
            return existingRegion;

        }
    }
}
