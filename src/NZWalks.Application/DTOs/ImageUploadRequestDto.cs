using System.ComponentModel.DataAnnotations;

namespace NZWalks.Application.DTOs
{
    public class ImageUploadRequestDto
    {
        [Required]
        public required Stream FileStream { get; set; } = null!;

        [Required]
        public required string FileName { get; set; }

        public string? FileDescription { get; set; }
    }
}
