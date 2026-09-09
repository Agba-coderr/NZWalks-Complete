using Microsoft.AspNetCore.Identity;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;

namespace NZWalks.Application.Interfaces.Services
{
    public interface IAuthenticationService
    {
        // Return IdentityResult so callers can surface specific Identity errors
        Task<Result> RegisterAsync(RegisterRequestDto registerRequestDto);

        Task<Result> VerifyEmailAsync(string userId, string token);

        Task<Result> LoginAsync(LoginRequestDto loginRequestDto);

        Task<Result> ResendVerificationEmailAsync(string email);
    }
}
