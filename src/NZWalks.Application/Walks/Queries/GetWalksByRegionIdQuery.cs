using AutoMapper;
using MediatR;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Walks.Queries
{
    public record GetWalksByRegionIdQuery
    (
        Guid RegionId, 
        int PageNumber = 1, 
        int PageSize = 10
    ) : IRequest<Result>;

    public class GetWalksByRegionIdQueryHandler : IRequestHandler<GetWalksByRegionIdQuery, Result>
    {
        private readonly IWalkRepository _walkRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly IMapper _mapper;
        

        public GetWalksByRegionIdQueryHandler(IWalkRepository walkRepository, IRegionRepository regionRepository, IMapper mapper)
        {
            _walkRepository = walkRepository;
            _regionRepository = regionRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(GetWalksByRegionIdQuery request, CancellationToken cancellationToken)
        {
            var validationResult = PaginationValidator.Validate(request.PageNumber, request.PageSize);

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var region = await _regionRepository.GetRegionByIdAsync(request.RegionId);

            if (region == null)
            {
                return Result.Failure($"Region with ID {request.RegionId} does not exist.", 404);
            }

            var (walks, totalCount) = await _walkRepository.GetWalksByRegionIdAsync(request.RegionId, request.PageNumber, request.PageSize);

            var pagedResponse = PagedResponse<WalkDto>.Create(_mapper.Map<List<WalkDto>>(walks), request.PageNumber, request.PageSize, totalCount);

            return Result.Success(pagedResponse, $"Walks for the region: {request.RegionId} retrieved successfully");
        }
    }
}
