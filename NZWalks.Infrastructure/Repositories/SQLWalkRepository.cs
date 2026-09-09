using NZWalks.Infrastructure.Data;
using NZWalks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NZWalks.Application.DTOs;
using NZWalks.Domain.Enums;
using NZWalks.Application.Extensions;
using NZWalks.Application.Interfaces.Repositories;


namespace NZWalks.Infrastructure.Repositories
{
    public class SQLWalkRepository : IWalkRepository
    {
        private readonly NZWalksDbContext _dbcontext;

        public SQLWalkRepository(NZWalksDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public async Task<Walk> CreateWalkAsync(Walk walk)
        {
            await _dbcontext.Walks.AddAsync(walk);
            await _dbcontext.SaveChangesAsync();
            return walk;
        }

        public async Task<Walk?> DeleteWalkAsync(Guid id)
        {
            var existingWalk = await _dbcontext.Walks.FindAsync(id);
            if (existingWalk == null)
            {
                return null;
            }

            _dbcontext.Walks.Remove(existingWalk);
            await _dbcontext.SaveChangesAsync();
            return existingWalk;
        }

        public async Task<(List<Walk> Walks, int TotalCount)> GetAllWalksAsync(string? filterOn = null, string? filterQuery = null, int pageNumber = 1, int pageSize = 10)
        {
            var walks = _dbcontext.Walks.Include(w => w.Region).AsQueryable();
            //Filtering
            if(string.IsNullOrWhiteSpace(filterOn) == false && string.IsNullOrWhiteSpace(filterQuery) == false)
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    walks = walks.Where(x => x.Name.Contains(filterQuery));
                }
            }

            var totalCount = await walks.CountAsync();

            var allWalks = await walks.Paginate(pageNumber, pageSize).ToListAsync();

            return (allWalks, totalCount);
        }

        public async Task<Walk?> GetWalkByIdAsync(Guid id)
        {
            return await _dbcontext.Walks.Include(w => w.Region).FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<(List<Walk> Walks, int TotalCount)> GetWalksByRegionIdAsync(Guid regionId, int pageNumber = 1, int pageSize = 10)
        {
            var walks = _dbcontext.Walks.Include(w => w.Region).Where(w => w.RegionId == regionId).AsQueryable();

            var totalCount = await walks.CountAsync();

            var walkList = await walks.Paginate(pageNumber, pageSize).ToListAsync();

            return (walkList, totalCount);
        }

        public async Task<(List<Walk> Walks, int TotalCount)> GetWalksByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            var walks = _dbcontext.Walks.Include(w => w.Region).Where(w => w.CreatedByUserId == userId).AsQueryable();

            var totalCount = await walks.CountAsync();

            var walkList = await walks.Paginate(pageNumber, pageSize).ToListAsync();

            return (walkList, totalCount);
        }

        public async Task<Walk?> GetLongestWalkByUserIdAsync(string userId)
        {
            return await _dbcontext.Walks.Include(w => w.Region).Where(w => w.CreatedByUserId == userId).OrderByDescending(w => w.LengthInKm).FirstOrDefaultAsync();
        }

        public async Task<Walk?> UpdateWalkAsync(Guid id, Walk walk)
        {
            var exisitingWalk = await _dbcontext.Walks.FindAsync(id);

            if (exisitingWalk == null)
            {
                return null;
            }

            exisitingWalk.Name = walk.Name;
            exisitingWalk.Description = walk.Description;
            exisitingWalk.LengthInKm = walk.LengthInKm;
            exisitingWalk.WalkImageUrl = walk.WalkImageUrl;
            exisitingWalk.DifficultyType = walk.DifficultyType;
            exisitingWalk.RegionId = walk.RegionId;

            await _dbcontext.SaveChangesAsync();
            return exisitingWalk;
        }

        public async Task<(List<Walk> Walks, int TotalCount)> GetWalksByDifficultyAsync(DifficultyType difficulty, int pageNumber = 1, int pageSize = 10)
        {
            var walks = _dbcontext.Walks.Include(w => w.Region).Where(w => w.DifficultyType == difficulty).AsQueryable();

            var totalCount = await walks.CountAsync();

            var walkList = await walks.Paginate(pageNumber, pageSize).ToListAsync();

            return (walkList, totalCount);
        }
    }
}
