//using AutoMapper;
//using NZWalks.Application.Common;
//using NZWalks.Domain.Entities;
//using NZWalks.Application.DTOs;
//using NZWalks.Domain.Enums;
//using NZWalks.Application.Interfaces.Repositories;
//using NZWalks.Application.Interfaces.Services;

//namespace NZWalks.Application.Services
//{
//    public class WalkService : IWalkService
//    {
//        private readonly IWalkRepository _walkRepository;
//        private readonly IMapper _mapper;
//        private readonly IRegionRepository _regionRepository;

//        public WalkService(IWalkRepository walkRepository, IMapper mapper, IRegionRepository regionRepository)
//        {
//            _walkRepository = walkRepository;
//            _mapper = mapper;
//            _regionRepository = regionRepository;
//        }

//        public async Task<Result> CreateWalkAsync(AddWalkRequestDto addWalkRequestDto, string createdByUserId)
//        {
//            var region = await _regionRepository.GetRegionByIdAsync(addWalkRequestDto.RegionId);

//            if (region == null)
//            {
//                return Result.Failure($"Region ID {addWalkRequestDto.RegionId} does not exist", 404);

//            }

//            var walk = new Walk
//            {
//                Name = addWalkRequestDto.Name,
//                Description = addWalkRequestDto.Description,
//                LengthInKm = addWalkRequestDto.LengthInKm,
//                WalkImageUrl = addWalkRequestDto.WalkImageUrl,
//                DifficultyType = addWalkRequestDto.DifficultyType,
//                RegionId = addWalkRequestDto.RegionId,
//                Region = region,
//                CreatedByUserId = createdByUserId
//            };

//            walk = await _walkRepository.CreateWalkAsync(walk);

//            return Result.Success(_mapper.Map<WalkDto>(walk), $"Walk created successfully.");
//        }

//        public async Task<Result> DeleteWalkAsync(Guid id, string currentUserId, bool isAdmin)
//        {
//            var existingWalk = await _walkRepository.GetWalkByIdAsync(id);

//            if (existingWalk == null)
//            {
//                return Result.Failure("This walk does not exist.", 404);
//            }

//            //var region = await _regionRepository.GetRegionByIdAsync(existingWalk.RegionId);
//            //if (region == null)
//            //{
//            //    return Result.Failure("The region associated with this walk no longer exists.", 400);
//            //}

//            if (!isAdmin && existingWalk.CreatedByUserId != currentUserId)
//            {
//                return Result.Failure("You do not own this walk.", 403);
//            }

//            var deleted = await _walkRepository.DeleteWalkAsync(id);

//            if (deleted == null)
//            {
//                return Result.Failure("This walk no longer exists.", 404);
//            }

//            return Result.Success(_mapper.Map<WalkDto>(deleted), "Walk deleted successfully");
//        }

//        public async Task<Result> GetAllWalksAsync(string? filterOn = null, string? filterQuery = null, int pageNumber = 1, int pageSize = 10)
//        {
//            var validationResult = PaginationValidator.Validate(pageNumber, pageSize);

//            if (!validationResult.IsSuccess)
//            {
//                return validationResult;
//            }

//            var (allWalks, totalCount) = await _walkRepository.GetAllWalksAsync(filterOn, filterQuery, pageNumber, pageSize);

//            var pagedResponse = PagedResponse<WalkDto>.Create(_mapper.Map<List<WalkDto>>(allWalks), pageNumber, pageSize, totalCount);

//            return Result.Success(pagedResponse, "Walks retrieved successfully");
//        }

//        public async Task<Result> GetLongestWalkByUserIdAsync(string userId)
//        {
//            var longestWalk = await _walkRepository.GetLongestWalkByUserIdAsync(userId);

//            if (longestWalk == null)
//            {
//                return Result.Failure($"Longest walk not found for user: {userId}", 404);
//            }

//            return Result.Success(_mapper.Map<WalkDto>(longestWalk), $"Longest walk for user: {userId} retrieved successfully");
//        }

//        public async Task<Result> GetWalkByIdAsync(Guid id)
//        {
//            var walk = await _walkRepository.GetWalkByIdAsync(id);

//            if (walk == null)
//            {
//                return Result.Failure($"Walk with ID {id} not found.", 404);
//            }

//            return Result.Success(_mapper.Map<WalkDto>(walk), $"Walk with ID {id} retrieved successfully");
//        }

//        public async Task<Result> GetWalksByDifficultyAsync(DifficultyType difficulty, int pageNumber = 1, int pageSize = 10)
//        {
//            var validationResult = PaginationValidator.Validate(pageNumber, pageSize);

//            if (!validationResult.IsSuccess)
//            {
//                return validationResult;
//            }

//            var (walks, totalCount) = await _walkRepository.GetWalksByDifficultyAsync(difficulty, pageNumber, pageSize);

//            var pagedResponse = PagedResponse<WalkDto>.Create(_mapper.Map<List<WalkDto>>(walks), pageNumber, pageSize, totalCount);

//            return Result.Success(pagedResponse, $"Walks with difficulty: {difficulty} retrieved successfully");
//        }

//        public async Task<Result> GetWalksByRegionIdAsync(Guid regionId, int pageNumber = 1, int pageSize = 10)
//        {
//            var validationResult = PaginationValidator.Validate(pageNumber, pageSize);

//            if (!validationResult.IsSuccess)
//            {
//                return validationResult;
//            }

//            var region = await _regionRepository.GetRegionByIdAsync(regionId);

//            if (region == null)
//            {
//                return Result.Failure($"Region with ID {regionId} does not exist.", 404);
//            }

//            var (walks, totalCount) = await _walkRepository.GetWalksByRegionIdAsync(regionId, pageNumber, pageSize);

//            var pagedResponse = PagedResponse<WalkDto>.Create(_mapper.Map<List<WalkDto>>(walks), pageNumber, pageSize, totalCount);

//            return Result.Success(pagedResponse, $"Walks for the region: {regionId} retrieved successfully");
//        }

//        public async Task<Result> GetWalksByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
//        {
//            var validationResult = PaginationValidator.Validate(pageNumber, pageSize);

//            if (!validationResult.IsSuccess)
//            {
//                return validationResult;
//            }

//            var (walks, totalCount) = await _walkRepository.GetWalksByUserIdAsync(userId, pageNumber, pageSize);

//            var pagedResponse = PagedResponse<WalkDto>.Create(_mapper.Map<List<WalkDto>>(walks), pageNumber, pageSize, totalCount);

//            return Result.Success(pagedResponse, $"Walks for the user: {userId} retrieved successfully");
//        }

//        public async Task<Result> UpdateWalkAsync(Guid id, UpdateWalkDto updateWalkDto, string currentUserId, bool isAdmin)
//        {
//            var existingWalk = await _walkRepository.GetWalkByIdAsync(id);

//            if (existingWalk == null)
//            {
//                return Result.Failure("This walk does not exist.", 404);
//            }

//            if (existingWalk.CreatedByUserId != currentUserId)
//            {
//                return Result.Failure("You do not own this walk.", 403);
//            }

//            var region = await _regionRepository.GetRegionByIdAsync(updateWalkDto.RegionId);
//            if (region == null)
//            {
//                return Result.Failure("Invalid region ID.", 404);
//            }

//            var walkDomainModel = _mapper.Map<Walk>(updateWalkDto);
//            walkDomainModel.Region = region;
//            walkDomainModel.CreatedByUserId = existingWalk.CreatedByUserId;

//            var updated = await _walkRepository.UpdateWalkAsync(id, walkDomainModel);

//            if (updated == null)
//            {
//                return Result.Failure($"Walk with ID {id} not found.", 404);
//            }

//            return Result.Success(_mapper.Map<WalkDto>(updated), "Walk updated successfully");
//        }
//    }
//}
