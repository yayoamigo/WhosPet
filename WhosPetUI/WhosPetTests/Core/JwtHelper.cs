using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using AlertaPatitasAPIUI.Helpers;
using WhosPetCore.Domain.Entities.Auth;
using WhosPetCore.Domain.Indentity;
using WhosPetCore.ServiceContracts;

namespace AlertaPatitasAPIUI.Tests.Helpers
{
    public class JwtTokenGeneratorTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IUserRoleService> _mockUserRoleService;
        private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;
        private readonly string _mockConnectionString;

        public JwtTokenGeneratorTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockUserRoleService = new Mock<IUserRoleService>();
            _mockRefreshTokenService = new Mock<IRefreshTokenService>();
            _mockConnectionString = "FakeConnectionString";

            // Setting up configuration mock
            _mockConfig.SetupGet(config => config["JwtConfig:Secret"]).Returns("LKFJuifjdnvhLiieheyhcvyybd7ghgbdbcchv");
            _mockConfig.SetupGet(config => config["JwtConfig:ExpireTime"]).Returns("00:30:00");

            // Setting up role service mock
            _mockUserRoleService.Setup(s => s.GetUserRoles(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "Admin" });
            _mockUserRoleService.Setup(s => s.GetRoles(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "Administrator" });

            // Setting up refresh token service mock
            _mockRefreshTokenService.Setup(s => s.AddRefreshToken(It.IsAny<RefreshToken>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        }
        [Fact]
        public async Task GenerateJwtToken_ShouldReturnAuthResponse_WithValidTokenAndRefreshToken()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "123",
                Email = "test@example.com"
            };

            var jwtTokenGenerator = new JwtTokenGenerator(_mockUserRoleService.Object, _mockRefreshTokenService.Object);

            // Act
            var result = await jwtTokenGenerator.GenerateJwtToken(user, _mockConfig.Object, _mockConnectionString);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Result);
            Assert.NotEmpty(result.Token);
            Assert.NotEmpty(result.RefreshToken);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(result.Token) as JwtSecurityToken;
            Assert.NotNull(jsonToken);
            Assert.Equal(user.Email, jsonToken.Subject);
            Assert.Contains(jsonToken.Claims, c => c.Type == "role" && c.Value == "Administrator");
        }
        [Fact]
        public async Task GenerateJwtToken_ShouldHaveCorrectExpirationTime()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "123",
                Email = "test@example.com"
            };

            var jwtTokenGenerator = new JwtTokenGenerator(_mockUserRoleService.Object, _mockRefreshTokenService.Object);

            // Act
            var result = await jwtTokenGenerator.GenerateJwtToken(user, _mockConfig.Object, _mockConnectionString);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Result);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(result.Token) as JwtSecurityToken;
            Assert.NotNull(jsonToken);
            Assert.Equal(DateTime.UtcNow.AddMinutes(30), jsonToken.ValidTo, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GenerateJwtToken_ShouldThrowException_WithInvalidSecretKey()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "123",
                Email = "test@example.com"
            };

            var invalidConfig = new Mock<IConfiguration>();
            invalidConfig.SetupGet(config => config["JwtConfig:Secret"]).Returns("short");
            invalidConfig.SetupGet(config => config["JwtConfig:ExpireTime"]).Returns("00:30:00");

            var jwtTokenGenerator = new JwtTokenGenerator(_mockUserRoleService.Object, _mockRefreshTokenService.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => jwtTokenGenerator.GenerateJwtToken(user, invalidConfig.Object, _mockConnectionString));
        }

    }

}
