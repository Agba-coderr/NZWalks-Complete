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
    public class UpdateRegionCommandHandlerTests
    {
        private readonly Mock<IRegionRepository> _mockRegionRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UpdateRegionCommandHandler _handler;

        public UpdateRegionCommandHandlerTests()
        {
            _mockRegionRepository = new Mock<IRegionRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new UpdateRegionCommandHandler(_mockRegionRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WhenRegionNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var command = new UpdateRegionCommand(regionId, "CHC", "Christchurch", null);
            var domainModel = new Region { Code = "CHC", Name = "Christchurch" };

            _mockMapper.Setup(m => m.Map<Region>(command))
                .Returns(domainModel);

            _mockRegionRepository.Setup(r => r.UpdateRegionAsync(regionId, domainModel))
                .ReturnsAsync((Region?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be($"Region with ID {regionId} was not found");
            Assert.Null(result.Data);

            _mockMapper.Verify(m => m.Map<RegionDto>(It.IsAny<Region>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRegionFound_UpdatesRegionAndReturnsSuccess()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var command = new UpdateRegionCommand(regionId, "CHC", "Christchurch Updated", "https://example.com/chc.jpg");

            var domainModel = new Region
            {
                Code = "CHC",
                Name = "Christchurch Updated",
                RegionImageUrl = "https://example.com/chc.jpg"
            };

            var updatedDomain = new Region
            {
                Id = regionId,
                Code = "CHC",
                Name = "Christchurch Updated",
                RegionImageUrl = "https://example.com/chc.jpg"
            };

            var expectedDto = new RegionDto
            {
                Id = regionId,
                Code = "CHC",
                Name = "Christchurch Updated",
                RegionImageUrl = "https://example.com/chc.jpg"
            };

            _mockMapper.Setup(m => m.Map<Region>(command))
                .Returns(domainModel);

            _mockRegionRepository.Setup(r => r.UpdateRegionAsync(regionId, domainModel))
                .ReturnsAsync(updatedDomain);

            _mockMapper.Setup(m => m.Map<RegionDto>(updatedDomain))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Region updated successfully");

            var data = result.Data as RegionDto;
            data.Should().NotBeNull();
            data.Should().BeEquivalentTo(expectedDto);

            _mockRegionRepository.Verify(r => r.UpdateRegionAsync(regionId, domainModel), Times.Once);
        }
    }
}
