using AutoMapper;
using NZWalks.Application.Common;
using NZWalks.Domain.Entities;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using NZWalks.Application.Mappings;

namespace NZWalks.Application.Services
{
    public class RegionService : IRegionService
    {
        private readonly IRegionRepository _regionRepository;
        private readonly IMapper _mapper;

        public RegionService(IRegionRepository regionRepository, IMapper mapper)
        {
            _regionRepository = regionRepository;
            _mapper = mapper;
        }

        public async Task<Result> CreateRegionAsync(AddRegionRequestDto addRegionRequestDto)
        {
            var region = _mapper.Map<Region>(addRegionRequestDto);
            var createdRegion = await _regionRepository.CreateRegionAsync(region);

            return Result.Success(_mapper.Map<RegionDto>(createdRegion), "Region created successfully");
        }

        public async Task<Result> DeleteRegionAsync(Guid id)
        {
            var deletedRegion = await _regionRepository.DeleteRegionAsync(id);

            if (deletedRegion == null)
            {
                return Result.Failure($"Region with ID {id} was not found", 404);
            }

            return Result.Success(_mapper.Map<RegionDto>(deletedRegion), "Region deleted successfully");
        }

        public async Task<Result> GetAllRegionsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var validationResult = PaginationValidator.Validate(pageNumber, pageSize);

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var (regions, totalCount) = await _regionRepository.GetAllRegionsAsync(pageNumber, pageSize);

            var pagedResponse = PagedResponse<RegionDto>.Create(_mapper.Map<List<RegionDto>>(regions), pageNumber, pageSize, totalCount);

            return Result.Success(pagedResponse, "Regions retrieved successfully");
        }

        public async Task<Result> GetRegionByIdAsync(Guid id)
        {
            var region = await _regionRepository.GetRegionByIdAsync(id);

            if (region == null)
            {
                return Result.Failure($"Region with ID {id} was not found", 404);
            }

            return Result.Success(_mapper.Map<RegionDto>(region), "Region retrieved successfully");
        }

        public async Task<Result> UpdateRegionAsync(Guid id, UpdateRegionDto updateRegionDto)
        {
            var region = _mapper.Map<Region>(updateRegionDto);
            var updatedRegion = await _regionRepository.UpdateRegionAsync(id, region);

            if (updatedRegion == null)
            {
                return Result.Failure($"Region with ID {id} was not found", 404);
            }

            return Result.Success(_mapper.Map<RegionDto>(updatedRegion), "Region updated successfully");
        }
    }
}

