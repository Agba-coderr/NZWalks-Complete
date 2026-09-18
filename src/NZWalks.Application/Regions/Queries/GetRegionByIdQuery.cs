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
    public record GetRegionByIdQuery
    (
        Guid Id
    ) : IRequest<Result>;

    public class GetRegionByIdQueryHandler : IRequestHandler<GetRegionByIdQuery, Result>
    {
        private readonly IRegionRepository _regionRepository;
        private readonly IMapper _mapper;

        public GetRegionByIdQueryHandler(IRegionRepository regionRepository, IMapper mapper)
        {
            _regionRepository = regionRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(GetRegionByIdQuery request, CancellationToken cancellationToken)
        {
            var region = await _regionRepository.GetRegionByIdAsync(request.Id);

            if (region == null)
            {
                return Result.Failure($"Region with ID {request.Id} was not found", 404);
            }

            return Result.Success(_mapper.Map<RegionDto>(region), "Region retrieved successfully");
        }
    }
}
