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
    public record GetWalkByIdQuery
    (
        Guid Id
    ) : IRequest<Result>;

    public class GetWalkByIdQueryHandler : IRequestHandler<GetWalkByIdQuery, Result>
    {
        private readonly IWalkRepository _walkRepository;
        private readonly IMapper _mapper;

        public GetWalkByIdQueryHandler(IWalkRepository walkRepository, IMapper mapper)
        {
            _walkRepository = walkRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(GetWalkByIdQuery request, CancellationToken cancellationToken)
        {
            var walk = await _walkRepository.GetWalkByIdAsync(request.Id);

            if (walk == null)
            {
                return Result.Failure($"Walk with ID {request.Id} not found.", 404);
            }

            return Result.Success(_mapper.Map<WalkDto>(walk), $"Walk with ID {request.Id} retrieved successfully");
        }
    }
}


    
