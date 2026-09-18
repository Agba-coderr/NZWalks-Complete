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
    public record CreateWalkCommand
    (
        string Name,
        string Description,
        double LengthInKm,
        string? WalkImageUrl,
        DifficultyType DifficultyType,
        Guid RegionId
    ) : IRequest<Result>;

    public class CreateWalkCommandHandler : IRequestHandler<CreateWalkCommand, Result>
    {
        private readonly IWalkRepository _walkRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        public CreateWalkCommandHandler(IWalkRepository walkRepository, IRegionRepository regionRepository, ICurrentUserService currentUser, IMapper mapper)
        {
            _walkRepository = walkRepository;
            _regionRepository = regionRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result> Handle(CreateWalkCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return Result.Failure("User is not authenticated", 401);
            }

            var region = await _regionRepository.GetRegionByIdAsync(request.RegionId);

            if (region == null)
            {
                return Result.Failure($"Region ID {request.RegionId} does not exist", 404);

            }

            var walk = new Walk
            {
                Name = request.Name,
                Description = request.Description,
                LengthInKm = request.LengthInKm,
                WalkImageUrl = request.WalkImageUrl,
                DifficultyType = request.DifficultyType,
                RegionId = request.RegionId,
                Region = region,
                CreatedByUserId = _currentUser.UserId,
            };

            walk = await _walkRepository.CreateWalkAsync(walk);

            return Result.Success(_mapper.Map<WalkDto>(walk), $"Walk created successfully.");
        }

    }
}
