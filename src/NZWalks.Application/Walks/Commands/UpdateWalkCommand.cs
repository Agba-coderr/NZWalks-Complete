using AutoMapper;
using MediatR;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using NZWalks.Domain.Entities;
using NZWalks.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Walks.Commands
{
    public record UpdateWalkCommand
    (
        Guid Id,
        string Name,
        string Description,
        double LengthInKm,
        string? WalkImageUrl,
        DifficultyType DifficultyType,
        Guid RegionId
    ) : IRequest<Result>;

    public class UpdateWalkCommandHandler : IRequestHandler<UpdateWalkCommand, Result>
    {
        private readonly IWalkRepository _walkRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public UpdateWalkCommandHandler(IWalkRepository walkRepository, IRegionRepository regionRepository, ICurrentUserService currentUser, IMapper mapper)
        {
            _walkRepository = walkRepository;
            _regionRepository = regionRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result> Handle(UpdateWalkCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return Result.Failure("User is not authenticated", 401);
            }

            var existingWalk = await _walkRepository.GetWalkByIdAsync(request.Id);

            if (existingWalk == null)
            {
                return Result.Failure("This walk does not exist.", 404);
            }

            if (existingWalk.CreatedByUserId != _currentUser.UserId)
            {
                return Result.Failure("You do not own this walk.", 403);
            }

            var region = await _regionRepository.GetRegionByIdAsync(request.RegionId);
            if (region == null)
            {
                return Result.Failure("Invalid region ID.", 404);
            }

            var walkDomainModel = _mapper.Map<Walk>(request);
            walkDomainModel.Region = region;
            walkDomainModel.CreatedByUserId = existingWalk.CreatedByUserId;

            var updated = await _walkRepository.UpdateWalkAsync(request.Id, walkDomainModel);

            if (updated == null)
            {
                return Result.Failure($"Walk with ID {request.Id} not found.", 404);
            }

            return Result.Success(_mapper.Map<WalkDto>(updated), "Walk updated successfully");
        }
    }
}
