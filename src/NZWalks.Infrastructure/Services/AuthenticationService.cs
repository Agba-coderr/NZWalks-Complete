using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NZWalks.Infrastructure.Data;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using System.Linq;
using System.Net;

namespace NZWalks.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly IEmailService _emailService;
        private readonly NZWalksAuthDbContext _nZWalksAuthDbContext;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration? _configuration;

        public AuthenticationService(
            UserManager<IdentityUser> userManager,
            ITokenRepository tokenRepository,
            IEmailService emailService,
            NZWalksAuthDbContext nZWalksAuthDbContext,
            ILogger<AuthenticationService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration? configuration = null)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
            _emailService = emailService;
            _nZWalksAuthDbContext = nZWalksAuthDbContext;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        private string GetBaseUrl()
        {
            // 1. Explicitly configured BaseUrl in appsettings.json
            var configuredUrl = _configuration?["EmailSettings:BaseUrl"] ?? _configuration?["AppUrl"];
            if (!string.IsNullOrWhiteSpace(configuredUrl))
            {
                return configuredUrl.TrimEnd('/');
            }

            // 2. Read from active request (handling X-Forwarded headers from ngrok/proxies)
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                var scheme = request.Headers.TryGetValue("X-Forwarded-Proto", out var proto) && !string.IsNullOrWhiteSpace(proto)
                    ? proto.ToString()
                    : request.Scheme;

                var host = request.Headers.TryGetValue("X-Forwarded-Host", out var forwardedHost) && !string.IsNullOrWhiteSpace(forwardedHost)
                    ? forwardedHost.ToString()
                    : request.Host.ToString();

                return $"{scheme}://{host}";
            }

            return "https://localhost:7246";
        }

        public async Task<Result> RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            // 1. Begin Database Transaction
            using var transaction = await _nZWalksAuthDbContext.Database.BeginTransactionAsync();

            try
            {
                var identityUser = new IdentityUser
                {
                    UserName = registerRequestDto.Username,
                    Email = registerRequestDto.Username
                };

                // 2. Create the user in Identity (EmailConfirmed is false by default)
                var createResult = await _userManager.CreateAsync(identityUser, registerRequestDto.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return Result.Failure(errors, 400);
                }

                // 3. Assign roles chosen by the user (require at least one)
                var roles = registerRequestDto.Roles?
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .ToArray();
                if (roles == null || roles.Length == 0)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure("At least one role is required.", 400);
                }

                // Prevent users from self-assigning Admin
                var forbidden = new[] { "Admin" };
                if (roles.Any(r => forbidden.Contains(r, StringComparer.OrdinalIgnoreCase)))
                {
                    await transaction.RollbackAsync();
                    return Result.Failure("You cannot assign the Admin role.", 403);
                }

                var roleResult = await _userManager.AddToRolesAsync(identityUser, roles);
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return Result.Failure(errors, 400);
                }

                // 4. Generate email verification token and build dynamic link
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                var encodedToken = WebUtility.UrlEncode(token);

                var baseUrl = GetBaseUrl();
                var confirmationLink = $"{baseUrl}/api/Auth/VerifyEmail?userId={identityUser.Id}&token={encodedToken}";

                // 5. Send Verification Email
                var emailBody = $@"
                    <p>Hi <b>{identityUser.UserName}</b>,</p>
                    <p>Welcome to NZ Walks! Please confirm your email address by clicking the link below:</p>
                    <p><a href='{confirmationLink}' style='padding: 10px 15px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Verify My Email</a></p>
                    <br/>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{confirmationLink}</p>
                    <br/>
                    <p>Best regards,<br/><b>The NZ Walks Team</b></p>";

                await _emailService.SendEmailAsync(
                    toEmail: identityUser.Email,
                    subject: "Verify Your Email - NZ Walks",
                    body: emailBody
                );

                // 6. Commit transaction ONLY after email is sent successfully
                await transaction.CommitAsync();

                return Result.Success(null, "Registration successful! Please check your email to verify your account.", 201);
            }
            catch (Exception ex)
            {
                // Rollback database changes if email sending fails or unexpected exception occurs
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Registration failed during email sending for {Email}", registerRequestDto.Username);

                return Result.Failure(
                    "We were unable to send your verification email. Registration could not be completed. Please try again in a few moments.",
                    500
                );
            }
        }

        public async Task<Result> VerifyEmailAsync(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return Result.Failure("Invalid verification link parameters.", 400);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found.", 404);
            }

            if (user.EmailConfirmed)
            {
                return Result.Success(null, "Email is already verified. You can log in.", 200);
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Email verification failed: {errors}", 400);
            }

            return Result.Success(null, "Email verified successfully! You can now log in.", 200);
        }

        public async Task<Result> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDto.Username);
            if (user == null)
            {
                return Result.Failure("Username or password incorrect", 400);
            }

            var checkPasswordResult = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if (!checkPasswordResult)
            {
                return Result.Failure("Username or password incorrect", 400);
            }

            // Block login if email is not verified yet
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Result.Failure("Please verify your email address before logging in.", 401);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = _tokenRepository.CreateJWTToken(user, roles.ToList());

            // Optional: send login notification email safely (non-blocking)
            try
            {
                await _emailService.SendEmailAsync(
                    toEmail: user.Email!,
                    subject: "New Login to Your NZ Walks Account",
                    body: $@"
                        <p>Hi <b>{user.UserName}</b>,</p>
                        <p>We noticed a new login to your NZ Walks account.</p>
                        <p>If this was you, no action is required.</p>
                        <p>If not, please secure your account immediately.</p>
                        <br/>
                        <p>Best regards,<br/><b>The NZ Walks Team</b></p>"
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not send login notification email to {Email}", user.Email);
            }

            var loginResponse = new LoginResponseDto
            {
                Token = jwtToken,
                UserId = user.Id,
                Roles = roles.ToList()
            };

            return Result.Success(loginResponse, "Login successful", 200);
        }

        public async Task<Result> ResendVerificationEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure("Email is required.", 400);
            }
            // 1. Find user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result.Failure("User with this email does not exist.", 404);
            }
            // 2. Check if already verified
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return Result.Failure("This email is already verified. Please log in.", 400);
            }
            try
            {
                // 3. Generate a new verification token & build the link
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);
                var baseUrl = GetBaseUrl();
                var confirmationLink = $"{baseUrl}/api/Auth/VerifyEmail?userId={user.Id}&token={encodedToken}";
                var emailBody = $@"
                    <p>Hi <b>{user.UserName}</b>,</p>
                    <p>You requested a new verification link for NZ Walks. Please confirm your email address by clicking the link below:</p>
                    <p><a href='{confirmationLink}' style='padding: 10px 15px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Verify My Email</a></p>
                    <br/>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{confirmationLink}</p>
                    <br/>
                    <p>Best regards,<br/><b>The NZ Walks Team</b></p>";

                // 4. Send the email
                await _emailService.SendEmailAsync(
                    toEmail: user.Email!,
                    subject: "Resend Email Verification - NZ Walks",
                    body: emailBody
                );
                return Result.Success(null, "A new verification email has been sent. Please check your inbox.", 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend verification email to {Email}", email);
                return Result.Failure("Unable to send verification email. Please try again in a few moments.", 500);
            }
        }
    }
}