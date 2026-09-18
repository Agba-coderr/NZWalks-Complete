using AutoMapper;
using MediatR;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Regions.Commands
{
    public record CreateRegionCommand
    (
        string Code,
        string Name,
        string? RegionImageUrl
    ) : IRequest<Result>;

    public class CreateRegionCommandHandler : IRequestHandler<CreateRegionCommand, Result>
    {
        private readonly IRegionRepository _regionRepository;
        private readonly IMapper _mapper;

        public CreateRegionCommandHandler(IRegionRepository regionRepository, IMapper mapper)
        {
            _regionRepository = regionRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(CreateRegionCommand request, CancellationToken cancellationToken)
        {
            var region = _mapper.Map<Region>(request);
            var createdRegion = await _regionRepository.CreateRegionAsync(region, cancellationToken);

            return Result.Success(_mapper.Map<RegionDto>(createdRegion), "Region created successfully");
        }
    }
}
