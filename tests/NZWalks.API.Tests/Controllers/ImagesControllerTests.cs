using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NZWalks.APIs.Controllers;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Domain.Entities;
using Xunit;

namespace NZWalks.API.Tests.Controllers
{
    public class ImagesControllerTests
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IImageRepository> _mockImageRepository;
        private readonly ImagesController _controller;

        public ImagesControllerTests()
        {
            _mockMapper = new Mock<IMapper>();
            _mockImageRepository = new Mock<IImageRepository>();
            _controller = new ImagesController(_mockMapper.Object, _mockImageRepository.Object);
        }

        [Fact]
        public async Task Upload_WhenValidRequest_MapsAndUploadsAndReturnsOkResult()
        {
            // Arrange
            var stream = new MemoryStream(new byte[1024]);
            var request = new ImageUploadRequestDto
            {
                FileStream = stream,
                FileName = "test.jpg",
                FileDescription = "Test Image"
            };

            var imageDomain = new Image
            {
                Id = Guid.NewGuid(),
                FileName = "test.jpg",
                FileDescription = "Test Image",
                FileExtension = ".jpg",
                FileSizeInBytes = 1024,
                FilePath = "https://example.com/images/test.jpg"
            };

            _mockMapper.Setup(m => m.Map<Image>(request))
                .Returns(imageDomain);

            _mockImageRepository.Setup(r => r.Upload(imageDomain, stream))
                .ReturnsAsync(imageDomain);

            // Act
            var actionResult = await _controller.Upload(request);

            // Assert
            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(imageDomain);

            _mockMapper.Verify(m => m.Map<Image>(request), Times.Once);
            _mockImageRepository.Verify(r => r.Upload(imageDomain, stream), Times.Once);
        }
    }
}
