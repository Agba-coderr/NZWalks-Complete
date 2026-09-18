using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NZWalks.Infrastructure.Data;
using NZWalks.Infrastructure.Services;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using System.Security.Claims;
using Xunit;

namespace NZWalks.API.Tests.Services
{
    public class AuthenticationServiceTests : IDisposable
    {
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<ITokenRepository> _mockTokenRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly SqliteConnection _sqliteConnection;
        private readonly NZWalksAuthDbContext _dbContext;

        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            // 1. Mock UserManager
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // 2. Mock Token Repo, Email Service, Logger
            _mockTokenRepository = new Mock<ITokenRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<AuthenticationService>>();

            // 3. Setup Mock HttpContextAccessor with a dummy request
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("nzwalks.example.com");
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            // 4. In-Memory SQLite DbContext to support transactions (BeginTransactionAsync)
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _sqliteConnection.Open();
            var dbOptions = new DbContextOptionsBuilder<NZWalksAuthDbContext>()
                .UseSqlite(_sqliteConnection)
                .Options;
            _dbContext = new NZWalksAuthDbContext(dbOptions);
            _dbContext.Database.EnsureCreated();

            // 5. System Under Test (SUT)
            _authService = new AuthenticationService(
                _mockUserManager.Object,
                _mockTokenRepository.Object,
                _mockEmailService.Object,
                _dbContext,
                _mockLogger.Object,
                _mockHttpContextAccessor.Object
            );
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _sqliteConnection.Dispose();
        }

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WhenUserCreationFails_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "test@example.com",
                Password = "Password123!",
                Roles = new[] { "Reader" }
            };

            var identityErrors = new[] { new IdentityError { Description = "Password is too weak." } };
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Contain("Password is too weak.");
        }

        [Fact]
        public async Task RegisterAsync_WhenNoRolesProvided_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "test@example.com",
                Password = "Password123!",
                Roles = Array.Empty<string>()
            };

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be("At least one role is required.");
        }

        [Fact]
        public async Task RegisterAsync_WhenUserAttemptsToSelfAssignAdmin_ReturnsForbiddenResult()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "admin@example.com",
                Password = "Password123!",
                Roles = new[] { "Admin", "Reader" }
            };

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(403);
            result.Message.Should().Be("You cannot assign the Admin role.");
        }

        [Fact]
        public async Task RegisterAsync_WhenAddingRolesFails_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "user@example.com",
                Password = "Password123!",
                Roles = new[] { "Writer" }
            };

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            var identityErrors = new[] { new IdentityError { Description = "Role does not exist." } };
            _mockUserManager.Setup(m => m.AddToRolesAsync(It.IsAny<IdentityUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Contain("Role does not exist.");
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailServiceThrowsException_ReturnsServerErrorAndRollsBack()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "user@example.com",
                Password = "Password123!",
                Roles = new[] { "Reader" }
            };

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(m => m.AddToRolesAsync(It.IsAny<IdentityUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync("token-123");

            _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP Server offline"));

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(500);
            result.Message.Should().Contain("We were unable to send your verification email.");
        }

        [Fact]
        public async Task RegisterAsync_WhenValid_SendsEmailAndReturnsCreatedSuccess()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "valid@example.com",
                Password = "Password123!",
                Roles = new[] { "Reader", "Writer" }
            };

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(m => m.AddToRolesAsync(It.IsAny<IdentityUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync("token-xyz");

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(201);
            result.Message.Should().Be("Registration successful! Please check your email to verify your account.");

            _mockEmailService.Verify(e => e.SendEmailAsync(
                "valid@example.com",
                "Verify Your Email - NZ Walks",
                It.Is<string>(body => body.Contains("https://nzwalks.example.com/api/Auth/VerifyEmail"))),
                Times.Once);
        }

        #endregion

        #region VerifyEmailAsync Tests

        [Theory]
        [InlineData("", "token")]
        [InlineData("userId", "")]
        [InlineData(null, "token")]
        [InlineData("userId", null)]
        public async Task VerifyEmailAsync_WhenParametersInvalid_ReturnsBadRequest(string? userId, string? token)
        {
            // Act
            var result = await _authService.VerifyEmailAsync(userId!, token!);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be("Invalid verification link parameters.");
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenUserNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockUserManager.Setup(m => m.FindByIdAsync("non-existent-user"))
                .ReturnsAsync((IdentityUser?)null);

            // Act
            var result = await _authService.VerifyEmailAsync("non-existent-user", "valid-token");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenUserAlreadyVerified_ReturnsSuccess()
        {
            // Arrange
            var user = new IdentityUser { Id = "user-1", EmailConfirmed = true };
            _mockUserManager.Setup(m => m.FindByIdAsync("user-1"))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.VerifyEmailAsync("user-1", "any-token");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Email is already verified. You can log in.");
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenConfirmationFails_ReturnsBadRequest()
        {
            // Arrange
            var user = new IdentityUser { Id = "user-1", EmailConfirmed = false };
            _mockUserManager.Setup(m => m.FindByIdAsync("user-1"))
                .ReturnsAsync(user);

            var identityErrors = new[] { new IdentityError { Description = "Invalid token." } };
            _mockUserManager.Setup(m => m.ConfirmEmailAsync(user, "bad-token"))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            var result = await _authService.VerifyEmailAsync("user-1", "bad-token");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Contain("Invalid token.");
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenConfirmationSucceeds_ReturnsSuccess()
        {
            // Arrange
            var user = new IdentityUser { Id = "user-1", EmailConfirmed = false };
            _mockUserManager.Setup(m => m.FindByIdAsync("user-1"))
                .ReturnsAsync(user);

            _mockUserManager.Setup(m => m.ConfirmEmailAsync(user, "valid-token"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.VerifyEmailAsync("user-1", "valid-token");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Email verified successfully! You can now log in.");
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WhenUserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "unknown@example.com", Password = "Password123!" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(request.Username))
                .ReturnsAsync((IdentityUser?)null);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be("Username or password incorrect");
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIncorrect_ReturnsBadRequest()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "user@example.com", Password = "WrongPassword" };
            var user = new IdentityUser { Id = "user-1", Email = request.Username };

            _mockUserManager.Setup(m => m.FindByEmailAsync(request.Username))
                .ReturnsAsync(user);

            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be("Username or password incorrect");
        }

        [Fact]
        public async Task LoginAsync_WhenEmailNotConfirmed_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "user@example.com", Password = "CorrectPassword" };
            var user = new IdentityUser { Id = "user-1", Email = request.Username };

            _mockUserManager.Setup(m => m.FindByEmailAsync(request.Username))
                .ReturnsAsync(user);

            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(401);
            result.Message.Should().Be("Please verify your email address before logging in.");
        }

        [Fact]
        public async Task LoginAsync_WhenValidCredentials_ReturnsSuccessWithJwtToken()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "user@example.com", Password = "CorrectPassword" };
            var user = new IdentityUser { Id = "user-1", UserName = "user@example.com", Email = "user@example.com" };
            var roles = new List<string> { "Reader", "Writer" };

            _mockUserManager.Setup(m => m.FindByEmailAsync(request.Username))
                .ReturnsAsync(user);

            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user))
                .ReturnsAsync(true);

            _mockUserManager.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _mockTokenRepository.Setup(t => t.CreateJWTToken(user, roles))
                .Returns("fake-jwt-token-string");

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("Login successful");

            var responseData = result.Data as LoginResponseDto;
            responseData.Should().NotBeNull();
            responseData!.Token.Should().Be("fake-jwt-token-string");
            responseData.UserId.Should().Be("user-1");
            responseData.Roles.Should().BeEquivalentTo(roles);
        }

        #endregion

        #region ResendVerificationEmailAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task ResendVerificationEmailAsync_WhenEmailEmpty_ReturnsBadRequest(string? email)
        {
            // Act
            var result = await _authService.ResendVerificationEmailAsync(email!);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be("Email is required.");
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_WhenUserNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockUserManager.Setup(m => m.FindByEmailAsync("notfound@example.com"))
                .ReturnsAsync((IdentityUser?)null);

            // Act
            var result = await _authService.ResendVerificationEmailAsync("notfound@example.com");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(404);
            result.Message.Should().Be("User with this email does not exist.");
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_WhenAlreadyVerified_ReturnsBadRequest()
        {
            // Arrange
            var user = new IdentityUser { Id = "user-1", Email = "verified@example.com" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.ResendVerificationEmailAsync(user.Email);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(400);
            result.Message.Should().Be("This email is already verified. Please log in.");
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_WhenEmailSendingFails_ReturnsServerError()
        {
            // Arrange
            var user = new IdentityUser { Id = "user-1", UserName = "testuser", Email = "unverified@example.com" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user))
                .ReturnsAsync(false);

            _mockUserManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync("token-xyz");

            _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP connection failure"));

            // Act
            var result = await _authService.ResendVerificationEmailAsync(user.Email);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(500);
            result.Message.Should().Be("Unable to send verification email. Please try again in a few moments.");
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_WhenValid_SendsEmailAndReturnsSuccess()
        {
            // Arrange
            var user = new IdentityUser { Id = "user-1", UserName = "testuser", Email = "unverified@example.com" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user))
                .ReturnsAsync(false);

            _mockUserManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync("token-12345");

            // Act
            var result = await _authService.ResendVerificationEmailAsync(user.Email);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Status.Should().Be(200);
            result.Message.Should().Be("A new verification email has been sent. Please check your inbox.");

            _mockEmailService.Verify(e => e.SendEmailAsync(
                user.Email,
                "Resend Email Verification - NZ Walks",
                It.Is<string>(body => body.Contains("https://nzwalks.example.com/api/Auth/VerifyEmail"))),
                Times.Once);
        }

        #endregion
    }
}
