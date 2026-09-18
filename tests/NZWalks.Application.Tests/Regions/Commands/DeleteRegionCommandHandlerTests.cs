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
    public class DeleteRegionCommandHandlerTests
    {
        private readonly Mock<IRegionRepository> _mockRegionRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DeleteRegionCommandHandler _handler;

        public DeleteRegionCommandHandlerTests()
        {
            _mockRegionRepository = new Mock<IRegionRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new DeleteRegionCommandHandler(_mockRegionRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WhenRegionNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var command = new DeleteRegionCommand(regionId);

            _mockRegionRepository.Setup(r => r.DeleteRegionAsync(regionId))
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
        public async Task Handle_WhenRegionFound_DeletesRegionAndReturnsSuccess()
        {
            // Arrange
            var regionId = Guid.NewGuid();
            var command = new DeleteRegionCommand(regionId);

            var deletedRegion = new Region
            {
                Id = regionId,
                Code = "BOP",
                Name = "Bay of Plenty"
            };

            var expectedDto = new RegionDto
            {
                Id = regionId,
                Code = "BOP",
                Name = "Bay of Plenty"
            };

            _mockRegionRepository.Setup(r => r.DeleteRegionAsync(regionId))
                .ReturnsAsync(deletedRegion);

            _mockMapper.Setup(m => m.Map<RegionDto>(deletedRegion))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Region deleted successfully");

            var data = result.Data as RegionDto;
            data.Should().NotBeNull();
            data.Should().BeEquivalentTo(expectedDto);

            _mockRegionRepository.Verify(r => r.DeleteRegionAsync(regionId), Times.Once);
        }
    }
}
