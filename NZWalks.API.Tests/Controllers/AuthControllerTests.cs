using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NZWalks.APIs.Controllers;
using NZWalks.Application.Common;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Services;
using Xunit;

namespace NZWalks.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthenticationService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Register_DelegatesToAuthService_ReturnsCorrectStatusCode()
        {
            // Arrange
            var request = new RegisterRequestDto { Username = "test@example.com", Password = "Password123!" };
            var expectedResponse = Result.Success(null, "Registration successful", 201);

            _mockAuthService.Setup(s => s.RegisterAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.Register(request);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task VerifyEmail_DelegatesToAuthService_ReturnsCorrectStatusCode()
        {
            // Arrange
            var expectedResponse = Result.Success(null, "Email verified", 200);
            _mockAuthService.Setup(s => s.VerifyEmailAsync("user-1", "token-xyz"))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.VerifyEmail("user-1", "token-xyz");

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Login_DelegatesToAuthService_ReturnsCorrectStatusCode()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "test@example.com", Password = "Password123!" };
            var expectedResponse = Result.Success(new LoginResponseDto { Token = "jwt" }, "Login successful", 200);

            _mockAuthService.Setup(s => s.LoginAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.Login(request);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ResendVerificationEmail_DelegatesToAuthService_ReturnsCorrectStatusCode()
        {
            // Arrange
            var request = new ResendVerificationEmailRequestDto { Email = "test@example.com" };
            var expectedResponse = Result.Success(null, "Sent", 200);

            _mockAuthService.Setup(s => s.ResendVerificationEmailAsync(request.Email))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.ResendVerificationEmail(request);

            // Assert
            var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
