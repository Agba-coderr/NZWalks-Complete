using System.ComponentModel.DataAnnotations;

namespace NZWalks.Application.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public required string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        // make nullable so public registration doesn't have to supply roles
        public string[]? Roles { get; set; }
    }
}