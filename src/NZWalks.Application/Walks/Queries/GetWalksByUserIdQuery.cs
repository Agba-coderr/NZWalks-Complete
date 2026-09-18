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
    public record GetWalksByUserIdQuery
    (
        int PageNumber = 1, 
        int PageSize = 10
    ) : IRequest<Result>;

    public class GetWalksByUserIdQueryHandler : IRequestHandler<GetWalksByUserIdQuery, Result>
    {
        private readonly IWalkRepository _walkRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public GetWalksByUserIdQueryHandler(IWalkRepository walkRepository, IMapper mapper, ICurrentUserService currentUser)
        {
            _walkRepository = walkRepository;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(GetWalksByUserIdQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return Result.Failure("User is not authenticated", 401);
            }

            var validationResult = PaginationValidator.Validate(request.PageNumber, request.PageSize);

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var (walks, totalCount) = await _walkRepository.GetWalksByUserIdAsync(_currentUser.UserId, request.PageNumber, request.PageSize);

            var pagedResponse = PagedResponse<WalkDto>.Create(_mapper.Map<List<WalkDto>>(walks), request.PageNumber, request.PageSize, totalCount);

            return Result.Success(pagedResponse, $"Walks for the user: {_currentUser.UserId} retrieved successfully");
        }
    }
}
