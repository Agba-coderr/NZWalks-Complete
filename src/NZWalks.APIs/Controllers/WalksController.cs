using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZWalks.APIs.CustomActionFilters;
using MediatR;
using NZWalks.Application.Walks.Queries;
using NZWalks.Application.Walks.Commands;

namespace NZWalks.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalksController : ControllerBase
    {
        private readonly ISender _sender;

        public WalksController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetAllWalks([FromQuery] GetAllWalksQuery query)
        {
            
            var result = await _sender.Send(query);

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetWalkById([FromRoute] Guid id)
        {
            var result = await _sender.Send(new GetWalkByIdQuery(id));

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("user")]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> GetWalksByUserId([FromQuery] GetWalksByUserIdQuery query)
        {
            var result = await _sender.Send(query);

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("user/longest")]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> GetLongestWalkByUserId()
        {
            var result = await _sender.Send(new GetLongestWalkByUserIdQuery());

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("region/{regionId:Guid}")]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetWalksByRegionId([FromRoute] Guid regionId, [FromQuery] GetWalksByRegionIdQuery query)
        {
            var result = await _sender.Send(query with { RegionId = regionId });

            return StatusCode(result.Status, result);
        }

        [HttpGet]
        [Route("difficulty")]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetWalksByDifficulty([FromQuery] GetWalksByDifficultyQuery query)
        {
            var result = await _sender.Send(query);

            return StatusCode(result.Status, result);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> CreateWalk([FromBody] CreateWalkCommand command)
        {
            var result = await _sender.Send(command);

            return StatusCode(result.Status, result);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [ValidateModel]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> UpdateWalk([FromRoute] Guid id, [FromBody] UpdateWalkCommand command)
        {
            var result = await _sender.Send(command with { Id = id });

            return StatusCode(result.Status, result);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer,Admin")]
        public async Task<IActionResult> DeleteWalk(Guid id)
        {
            var result = await _sender.Send(new DeleteWalkCommand(id));

            return StatusCode(result.Status, result);
        }
    }
}