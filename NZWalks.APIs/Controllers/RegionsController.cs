using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.CustomActionFilters;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Services;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionsController : ControllerBase
    {
        private readonly IRegionService _regionService;

        public RegionsController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        [HttpGet]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetAllRegions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var results = await _regionService.GetAllRegionsAsync(pageNumber, pageSize);

            return StatusCode(results.Status, results);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Reader,Writer,Admin")]
        public async Task<IActionResult> GetRegionById([FromRoute] Guid id)
        {
            var result = await _regionService.GetRegionByIdAsync(id);

            return StatusCode(result.Status, result);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRegion([FromBody] AddRegionRequestDto addRegionRequestDto)
        {
            var result = await _regionService.CreateRegionAsync(addRegionRequestDto);

            return StatusCode(result.Status, result);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [ValidateModel]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRegion([FromRoute] Guid id, [FromBody] UpdateRegionDto updateRegionDto)
        {
            var result = await _regionService.UpdateRegionAsync(id, updateRegionDto);

            return StatusCode(result.Status, result);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRegion([FromRoute] Guid id)
        {
            var result = await _regionService.DeleteRegionAsync(id);

            return StatusCode(result.Status, result);
        }
    }
}

