using AutoMapper;
using MediatR;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Walks.Commands
{
    public record DeleteWalkCommand
    (
        Guid Id
    ) : IRequest<Result>;

    public class DeleteWalkCommandHandler : IRequestHandler<DeleteWalkCommand, Result>
    {
        private readonly IWalkRepository _walkRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        public DeleteWalkCommandHandler(IWalkRepository walkRepository, ICurrentUserService currentUser, IMapper mapper)
        {
            _walkRepository = walkRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result> Handle(DeleteWalkCommand request, CancellationToken cancellationToken)
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

            //var region = await _regionRepository.GetRegionByIdAsync(existingWalk.RegionId);
            //if (region == null)
            //{
            //    return Result.Failure("The region associated with this walk no longer exists.", 400);
            //}

            if (!_currentUser.IsAdmin && existingWalk.CreatedByUserId != _currentUser.UserId)
            {
                return Result.Failure("You do not own this walk.", 403);
            }

            var deleted = await _walkRepository.DeleteWalkAsync(request.Id);

            if (deleted == null)
            {
                return Result.Failure("This walk no longer exists.", 404);
            }

            return Result.Success(_mapper.Map<WalkDto>(deleted), "Walk deleted successfully");
        }

    }
}
