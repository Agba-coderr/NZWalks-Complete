using AutoMapper;
using MediatR;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Walks.Queries
{
    public record GetWalksByDifficultyQuery
    (
        DifficultyType Difficulty,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Result>;

    public class GetWalksByDifficultyQueryHandler : IRequestHandler<GetWalksByDifficultyQuery, Result>
    {
        private readonly IWalkRepository _walkRepository;
        private readonly IMapper _mapper;
        public GetWalksByDifficultyQueryHandler(IWalkRepository walkRepository, IMapper mapper)
        {
            _walkRepository = walkRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(GetWalksByDifficultyQuery request, CancellationToken cancellationToken)
        {
            var validationResult = PaginationValidator.Validate(request.PageNumber, request.PageSize);

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var (walks, totalCount) = await _walkRepository.GetWalksByDifficultyAsync(request.Difficulty, request.PageNumber, request.PageSize);

            var pagedResponse = PagedResponse<WalkDto>.Create(_mapper.Map<List<WalkDto>>(walks), request.PageNumber, request.PageSize, totalCount);

            return Result.Success(pagedResponse, $"Walks with difficulty: {request.Difficulty} retrieved successfully");
        }
    }
}
