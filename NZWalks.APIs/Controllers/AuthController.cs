using Azure;
using Microsoft.AspNetCore.Mvc;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Services;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        //POST: /api/Auth/Register
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            var response = await _authenticationService.RegisterAsync(registerRequestDto);

            return StatusCode(response.Status, response);
        }

        // GET: /api/Auth/VerifyEmail?userId=...&token=...
        [HttpGet]
        [Route("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var response = await _authenticationService.VerifyEmailAsync(userId, token);

            return StatusCode(response.Status, response);
        }

        //POST: /api/Auth/Login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var response = await _authenticationService.LoginAsync(loginRequestDto);

            return StatusCode(response.Status, response);
        }

        // POST: /api/Auth/ResendVerificationEmail
        [HttpPost]
        [Route("ResendVerificationEmail")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequestDto requestDto)
        {
            var response = await _authenticationService.ResendVerificationEmailAsync(requestDto.Email);
            return StatusCode(response.Status, response);
        }
    }
}