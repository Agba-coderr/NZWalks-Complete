using AutoMapper;
using MediatR;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Walks.Queries
{
    public record GetAllWalksQuery
    (
        string? FilterOn = null, 
        string? FilterQuery = null, 
        int PageNumber = 1, 
        int PageSize = 10
    ) : IRequest<Result>;

    public class GetAllWalksQueryHandler : IRequestHandler<GetAllWalksQuery, Result>
    {
        private readonly IWalkRepository _walkRepository;
        private readonly IMapper _mapper;

        public GetAllWalksQueryHandler(IWalkRepository walkRepository, IMapper mapper)
        {
            _walkRepository = walkRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(GetAllWalksQuery request, CancellationToken cancellationToken)
        {
            var validationResult = PaginationValidator.Validate(request.PageNumber, request.PageSize);

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var (allWalks, totalCount) = await _walkRepository.GetAllWalksAsync(request.FilterOn, request.FilterQuery, request.PageNumber, request.PageSize);

            var pagedResponse = PagedResponse<WalkDto>.Create(_mapper.Map<List<WalkDto>>(allWalks), request.PageNumber, request.PageSize, totalCount);

            return Result.Success(pagedResponse, "Walks retrieved successfully");
        }
    }
}
