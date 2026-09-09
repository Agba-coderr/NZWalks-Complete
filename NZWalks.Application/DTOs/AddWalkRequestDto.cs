using NZWalks.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace NZWalks.Application.DTOs
{
    public class AddWalkRequestDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "Name has to be a maximum of 100 characters")]
        public required string Name { get; set; }

        [Required]
        [MaxLength(1000, ErrorMessage = "Description has to be a maximum of 1000 characters")]
        public required string Description { get; set; }

        [Required]
        [Range(0, 50, ErrorMessage = "Length in km has to be between 0 and 50")]
        public double LengthInKm { get; set; }

        public string? WalkImageUrl { get; set; }

        [Required]
        public required DifficultyType DifficultyType { get; set; }

        [Required]
        public Guid RegionId { get; set; }
    }
}
