using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NZWalks.APIs.CustomActionFilters;
using NZWalks.Application.DTOs;
using NZWalks.Domain.Enums;
using NZWalks.Application.Common;
using NZWalks.Application.Interfaces.Services;
using System.Security.Claims;

namespace NZWalks.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalksController : ControllerBase
    {
        private readonly IWalkService _walkService;

        public WalksController(IWalkService walkService)
        {
            _walkService = walkService;
        }

        [HttpGet]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetAllWalks([FromQuery] string? filterOn, [FromQuery] string? filterQuery, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            
            var result = await _walkService.GetAllWalksAsync(filterOn, filterQuery, pageNumber, pageSize);

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetWalkById([FromRoute] Guid id)
        {
            var result = await _walkService.GetWalkByIdAsync(id);

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("user")]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> GetWalksByUserId([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                var failureResponse = Result.Failure("User is not authenticated", 401);
                
                return StatusCode(failureResponse.Status, failureResponse);
            }

            var result = await _walkService.GetWalksByUserIdAsync(userId, pageNumber, pageSize);

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("user/longest")]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> GetLongestWalkByUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                var failureResponse = Result.Failure("User is not authenticated", 401);

                return StatusCode(failureResponse.Status, failureResponse);
            }

            var result = await _walkService.GetLongestWalkByUserIdAsync(userId);

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("region/{regionId:Guid}")]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetWalksByRegionId([FromRoute] Guid regionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _walkService.GetWalksByRegionIdAsync(regionId, pageNumber, pageSize);

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("difficulty")]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetWalksByDifficulty(DifficultyType difficulty, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _walkService.GetWalksByDifficultyAsync(difficulty, pageNumber, pageSize);

            return StatusCode(result.Status, result);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> CreateWalk([FromBody] AddWalkRequestDto addWalkRequestDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                var failureResponse = Result.Failure("User is not authenticated", 401);

                return StatusCode(failureResponse.Status, failureResponse);
            }
            
            var result = await _walkService.CreateWalkAsync(addWalkRequestDto, userId);

            return StatusCode(result.Status, result);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [ValidateModel]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> UpdateWalk([FromRoute] Guid id, [FromBody] UpdateWalkDto updateWalkDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                var failureResponse = Result.Failure("User is not authenticated", 401);

                return StatusCode(failureResponse.Status, failureResponse);
            }

            var isAdmin = User.IsInRole("Admin");

            var result = await _walkService.UpdateWalkAsync(id, updateWalkDto, userId, isAdmin);

            return StatusCode(result.Status, result);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> DeleteWalk(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                var failureResponse = Result.Failure("User is not authenticated", 401);

                return StatusCode(failureResponse.Status, failureResponse);
            }

            var isAdmin = User.IsInRole("Admin");

            var result = await _walkService.DeleteWalkAsync(id, userId, isAdmin);

            return StatusCode(result.Status, result);
        }
    }
}