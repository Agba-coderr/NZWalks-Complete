using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZWalks.APIs.CustomActionFilters;
using NZWalks.Application.Regions.Commands;
using NZWalks.Application.Regions.Queries;

namespace NZWalks.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionsController : ControllerBase
    {
        private readonly ISender _sender;

        public RegionsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetAllRegions([FromQuery] GetAllRegionsQuery query)
        {
            var results = await _sender.Send(query);

            return StatusCode(results.Status, results);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetRegionById([FromRoute] Guid id)
        {
            var result = await _sender.Send(new GetRegionByIdQuery(id));

            return StatusCode(result.Status, result);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRegion([FromBody] CreateRegionCommand command)
        {
            var result = await _sender.Send(command);

            return StatusCode(result.Status, result);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [ValidateModel]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRegion([FromRoute] Guid id, [FromBody] UpdateRegionCommand command)
        {
            var result = await _sender.Send(command with { Id = id });

            return StatusCode(result.Status, result);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRegion([FromRoute] Guid id)
        {
            var result = await _sender.Send(new DeleteRegionCommand(id));

            return StatusCode(result.Status, result);
        }
    }
}

