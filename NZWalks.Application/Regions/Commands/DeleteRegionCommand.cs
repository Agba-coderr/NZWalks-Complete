using AutoMapper;
using MediatR;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Regions.Commands
{
    public record DeleteRegionCommand
    (
        Guid Id
    ) : IRequest<Result>;

    public class DeleteRegionCommandHandler : IRequestHandler<DeleteRegionCommand, Result>
    {
        private readonly IRegionRepository _regionRepository;

        private readonly IMapper _mapper;

        public DeleteRegionCommandHandler(IRegionRepository regionRepository, IMapper mapper)
        {
            _regionRepository = regionRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(DeleteRegionCommand request, CancellationToken cancellationToken)
        {
            var deletedRegion = await _regionRepository.DeleteRegionAsync(request.Id);

            if (deletedRegion == null)
            {
                return Result.Failure($"Region with ID {request.Id} was not found", 404);
            }

            return Result.Success(_mapper.Map<RegionDto>(deletedRegion), "Region deleted successfully");
        }
    }

}
