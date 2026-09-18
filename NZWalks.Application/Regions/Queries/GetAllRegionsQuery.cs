using AutoMapper;
using MediatR;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Regions.Queries
{
    public record GetAllRegionsQuery
    (
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Result>;

    public class GetAllRegionsQueryHandler : IRequestHandler<GetAllRegionsQuery, Result>
    {
        private readonly IRegionRepository _regionRepository;
        private readonly IMapper _mapper;

        public GetAllRegionsQueryHandler(IRegionRepository regionRepository, IMapper mapper)
        {
            _regionRepository = regionRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(GetAllRegionsQuery request, CancellationToken cancellationToken)
        {
            var validationResult = PaginationValidator.Validate(request.PageNumber, request.PageSize);

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var (regions, totalCount) = await _regionRepository.GetAllRegionsAsync(request.PageNumber, request.PageSize);

            var pagedResponse = PagedResponse<RegionDto>.Create(_mapper.Map<List<RegionDto>>(regions), request.PageNumber, request.PageSize, totalCount);

            return Result.Success(pagedResponse, "Regions retrieved successfully");
        }
    }
}
