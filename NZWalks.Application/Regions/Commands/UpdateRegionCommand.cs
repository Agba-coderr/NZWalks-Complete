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
    public record UpdateRegionCommand
    (
        Guid Id,
        string Code,
        string Name,
        string? RegionImageUrl
    ) : IRequest<Result>;

    public class UpdateRegionCommandHandler : IRequestHandler<UpdateRegionCommand, Result>
    {
        private readonly IRegionRepository _regionRepository;
        private readonly IMapper _mapper;

        public UpdateRegionCommandHandler(IRegionRepository regionRepository, IMapper mapper)
        {
            _regionRepository = regionRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(UpdateRegionCommand request, CancellationToken cancellationToken)
        {
            var region = _mapper.Map<Region>(request);
            var updatedRegion = await _regionRepository.UpdateRegionAsync(request.Id, region);

            if (updatedRegion == null)
            {
                return Result.Failure($"Region with ID {request.Id} was not found", 404);
            }

            return Result.Success(_mapper.Map<RegionDto>(updatedRegion), "Region updated successfully");
        }

    }
}
