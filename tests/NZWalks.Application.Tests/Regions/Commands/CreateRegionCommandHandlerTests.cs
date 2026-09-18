using AutoMapper;
using FluentAssertions;
using Moq;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Regions.Commands;
using NZWalks.Domain.Entities;
using Xunit;

namespace NZWalks.API.Tests.Regions.Commands
{
    public class CreateRegionCommandHandlerTests
    {
        private readonly Mock<IRegionRepository> _mockRegionRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CreateRegionCommandHandler _handler;

        public CreateRegionCommandHandlerTests()
        {
            _mockRegionRepository = new Mock<IRegionRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new CreateRegionCommandHandler(_mockRegionRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WhenValidCommand_MapsToRegion_CallsRepository_ReturnsSuccessResultWithRegionDto()
        {
            // Arrange
            var command = new CreateRegionCommand("AKL", "Auckland", "https://example.com/akl.jpg");

            var domainModel = new Region
            {
                Id = Guid.NewGuid(),
                Code = "AKL",
                Name = "Auckland",
                RegionImageUrl = "https://example.com/akl.jpg"
            };

            var expectedDto = new RegionDto
            {
                Id = domainModel.Id,
                Code = domainModel.Code,
                Name = domainModel.Name,
                RegionImageUrl = domainModel.RegionImageUrl
            };

            _mockMapper.Setup(m => m.Map<Region>(command))
                .Returns(domainModel);

            _mockRegionRepository.Setup(r => r.CreateRegionAsync(domainModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(domainModel);

            _mockMapper.Setup(m => m.Map<RegionDto>(domainModel))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Region created successfully");

            var data = result.Data as RegionDto;
            data.Should().NotBeNull();
            data.Should().BeEquivalentTo(expectedDto);

            _mockRegionRepository.Verify(r => r.CreateRegionAsync(domainModel, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
