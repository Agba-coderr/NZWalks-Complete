using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NZWalks.Infrastructure.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace NZWalks.API.Tests.Repositories
{
    public class TokenRepositoryTests
    {
        private readonly IConfiguration _configuration;
        private readonly TokenRepository _repository;

        public TokenRepositoryTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsASecretKeyForTestingPurposesOnly1234567890!" },
                { "Jwt:Issuer", "https://nzwalks.example.com" },
                { "Jwt:Audience", "https://nzwalks.example.com" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _repository = new TokenRepository(_configuration);
        }

        [Fact]
        public void CreateJWTToken_WhenValidUserAndRoles_ReturnsValidJwtWithClaims()
        {
            // Arrange
            var user = new IdentityUser
            {
                Id = "user-12345",
                Email = "testuser@example.com"
            };
            var roles = new List<string> { "Reader", "Writer" };

            // Act
            var tokenString = _repository.CreateJWTToken(user, roles);

            // Assert
            tokenString.Should().NotBeNullOrWhiteSpace();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenString);

            jwtToken.Issuer.Should().Be("https://nzwalks.example.com");
            jwtToken.Audiences.Should().Contain("https://nzwalks.example.com");

            var claims = jwtToken.Claims.ToList();
            claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user-12345");
            claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "testuser@example.com");
            claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Reader");
            claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Writer");
        }

        [Fact]
        public void CreateJWTToken_WhenJwtKeyMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var emptyConfig = new ConfigurationBuilder().Build();
            var repo = new TokenRepository(emptyConfig);
            var user = new IdentityUser { Id = "user-1", Email = "test@example.com" };

            // Act
            var act = () => repo.CreateJWTToken(user, new List<string> { "Reader" });

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Jwt:Key configuration is missing.");
        }
    }
}
