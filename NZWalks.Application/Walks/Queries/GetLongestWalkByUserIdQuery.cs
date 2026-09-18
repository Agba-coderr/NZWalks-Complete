using AutoMapper;
using MediatR;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Walks.Queries
{
    public record GetLongestWalkByUserIdQuery() : IRequest<Result>;

    public class GetLongestWalkByUserIdQueryHandler : IRequestHandler<GetLongestWalkByUserIdQuery, Result>
    {
        private readonly IWalkRepository _walkRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public GetLongestWalkByUserIdQueryHandler(IWalkRepository walkRepository, IMapper mapper, ICurrentUserService currentUser)
        {
            _walkRepository = walkRepository;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(GetLongestWalkByUserIdQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return Result.Failure("User is not authenticated", 401);
            }

            var longestWalk = await _walkRepository.GetLongestWalkByUserIdAsync(_currentUser.UserId);

            if (longestWalk == null)
            {
                return Result.Failure($"Longest walk not found for user: {_currentUser.UserId}", 404);
            }

            return Result.Success(_mapper.Map<WalkDto>(longestWalk), $"Longest walk for user: {_currentUser.UserId} retrieved successfully");
        }
    }
}
