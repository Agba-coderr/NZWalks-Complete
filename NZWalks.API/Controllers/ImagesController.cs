using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.CustomActionFilters;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IImageRepository _imageRepository;

        public ImagesController(IMapper mapper, IImageRepository imageRepository)
        {
            _mapper = mapper;
            _imageRepository = imageRepository;
        }

        // POST: /api/Images/Upload
        [HttpPost]
        [Route("Upload")]
        [ValidateFileUpload]
        public async Task<IActionResult> Upload([FromForm] ImageUploadRequestDto request)
        {
            // 1. Map DTO to Domain Model using AutoMapper
            var imageDomainModel = _mapper.Map<Image>(request);

            // 2. Use Repository to save file and persist DB record
            await _imageRepository.Upload(imageDomainModel, request.File);

            return Ok(imageDomainModel);
        }
    }
}
