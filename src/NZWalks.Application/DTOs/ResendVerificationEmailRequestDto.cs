using System.ComponentModel.DataAnnotations;

namespace NZWalks.Application.DTOs
{
    public class ResendVerificationEmailRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}