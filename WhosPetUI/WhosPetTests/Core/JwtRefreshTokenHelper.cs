using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AlertaPatitasAPIUI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using WhosPetCore.Domain.Entities.Auth;
using WhosPetCore.Domain.Indentity;
using WhosPetCore.Domain.ServiceContracts;
using WhosPetCore.DTO.Incoming.Auth;
using WhosPetCore.Helpers;
using Xunit;

namespace AlertaPatitasAPIUI.Tests.Helpers
{
    public class JwtHelperTests
    {
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly Mock<IUserRoleService> _mockRoleService;
        private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly string _mockConnectionString = "FakeConnectionString";
        private readonly JwtHelper _jwtHelper;

        public JwtHelperTests()
        {
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("LKFJuifjdnvhLiieheyhcvyybd7ghgbdbcchv"))
            };

            _mockRoleService = new Mock<IUserRoleService>();
            _mockRefreshTokenService = new Mock<IRefreshTokenService>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                null, null, null, null, null, null, null, null
            );
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                null, null, null, null
            );
            _mockConfig = new Mock<IConfiguration>();

            _jwtHelper = new JwtHelper(
                _tokenValidationParameters,
                _mockConnectionString,
                _mockRoleService.Object,
                _mockRefreshTokenService.Object,
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockConfig.Object
            );
        }

        private string GenerateInvalidJwtToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("LKFJuifjdnvhLiieheyhcvyybd7ghgbdbcchv"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "testuser"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, DateTime.UtcNow.AddMinutes(-5).ToString()) // Expired token
            };

            var token = new JwtSecurityToken(
                issuer: "testissuer",
                audience: "testaudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(-5), // Token already expired
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateValidJwtToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("LKFJuifjdnvhLiieheyhcvyybd7ghgbdbcchv"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "testuser"),
                new Claim(JwtRegisteredClaimNames.Jti, "validJti"),
                new Claim(JwtRegisteredClaimNames.Exp, DateTime.UtcNow.AddMinutes(5).ToString()) // Token not expired
            };

            var token = new JwtSecurityToken(
                issuer: "testissuer",
                audience: "testaudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5), // Token expires in 5 minutes
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateValidExpiredJwtToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("LKFJuifjdnvhLiieheyhcvyybd7ghgbdbcchv"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "testuser"),
                new Claim(JwtRegisteredClaimNames.Jti, "validJti"),
                new Claim(JwtRegisteredClaimNames.Exp, DateTime.UtcNow.AddMinutes(5).ToString()) // Token not expired
            };

            var token = new JwtSecurityToken(
                issuer: "testissuer",
                audience: "testaudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(-5), // Expired token
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Fact]
        public async Task VeryfyAndGenerateToken_ShouldReturnServerErrorResponse_WhenTokenIsInvalid()
        {
            // Arrange
            var tokenRequest = new TokenRequestDTO
            {
                Token = GenerateInvalidJwtToken(),
                RefreshToken = "InvalidRefreshToken"
            };

            // Act
            var result = await _jwtHelper.VeryfyAndGenerateToken(tokenRequest);

            // Assert
            Assert.False(result.Result);
            Assert.Contains("Server Error", result.Errors);
        }

        [Fact]
        public async Task VeryfyAndGenerateToken_ShouldReturnTokenNotExpiredResponse_WhenTokenNotExpired()
        {
            // Arrange
            var tokenRequest = new TokenRequestDTO
            {
                Token = GenerateValidJwtToken(),
                RefreshToken = "ValidRefreshToken"
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            // Act
            var result = await _jwtHelper.VeryfyAndGenerateToken(tokenRequest);

            // Assert
            Assert.False(result.Result);
            Assert.Contains("This token has not expired yet", result.Errors);
        }
        [Fact]
        public async Task VeryfyAndGenerateToken_ShouldReturnRefreshTokenNotFound_WhenTokenIsValidButRefreshTokenNotFound()
        {
            // Arrange
            var tokenRequest = new TokenRequestDTO
            {
                Token = GenerateValidExpiredJwtToken(),
                RefreshToken = "NonExistentRefreshToken"
            };

            // Mock the GetRefreshTokenAsync method to return null
            var mockJwtHelper = new Mock<JwtHelper>(_tokenValidationParameters, _mockConnectionString, _mockRoleService.Object, _mockRefreshTokenService.Object, _mockUserManager.Object, _mockSignInManager.Object, _mockConfig.Object);
            mockJwtHelper.CallBase = true;
            mockJwtHelper.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken)null);

            // Act
            var result = await mockJwtHelper.Object.VeryfyAndGenerateToken(tokenRequest);

            // Assert
            Assert.False(result.Result);
            Assert.Contains("This refresh token doesn't exist", result.Errors);
        }

        [Fact]
        public async Task VeryfyAndGenerateToken_ShouldReturnRefreshTokenExpired_WhenTokenIsValidButRefreshTokenExpired()
        {
            // Arrange
            var tokenRequest = new TokenRequestDTO
            {
                Token = GenerateValidExpiredJwtToken(),
                RefreshToken = "ExpiredRefreshToken"
            };

            var expiredRefreshToken = new RefreshToken
            {
                jwtId = "validJti",
                IsUsed = false,
                IsRevoked = false,
                UserId = "testuser",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                ExpireDate = DateTime.UtcNow.AddDays(-5), // Already expired
                Token = "ExpiredRefreshToken"
            };

            // Mock the GetRefreshTokenAsync method to return expiredRefreshToken
            var mockJwtHelper = new Mock<JwtHelper>(_tokenValidationParameters, _mockConnectionString, _mockRoleService.Object, _mockRefreshTokenService.Object, _mockUserManager.Object, _mockSignInManager.Object, _mockConfig.Object);
            mockJwtHelper.CallBase = true;
            mockJwtHelper.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(expiredRefreshToken);

            // Act
            var result = await mockJwtHelper.Object.VeryfyAndGenerateToken(tokenRequest);

            // Assert
            Assert.False(result.Result);
            Assert.Contains("This refresh token has expired", result.Errors);
        }

        [Fact]
        public async Task VeryfyAndGenerateToken_ShouldReturnRefreshTokenUsed_WhenTokenIsValidButRefreshTokenUsed()
        {
            // Arrange
            var tokenRequest = new TokenRequestDTO
            {
                Token = GenerateValidExpiredJwtToken(),
                RefreshToken = "UsedRefreshToken"
            };

            var usedRefreshToken = new RefreshToken
            {
                jwtId = "validJti",
                IsUsed = true,
                IsRevoked = false,
                UserId = "testuser",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                ExpireDate = DateTime.UtcNow.AddDays(5),
                Token = "UsedRefreshToken"
            };

            // Mock the GetRefreshTokenAsync method to return usedRefreshToken
            var mockJwtHelper = new Mock<JwtHelper>(_tokenValidationParameters, _mockConnectionString, _mockRoleService.Object, _mockRefreshTokenService.Object, _mockUserManager.Object, _mockSignInManager.Object, _mockConfig.Object);
            mockJwtHelper.CallBase = true;
            mockJwtHelper.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(usedRefreshToken);

            // Act
            var result = await mockJwtHelper.Object.VeryfyAndGenerateToken(tokenRequest);

            // Assert
            Assert.False(result.Result);
            Assert.Contains("This refresh token has been used", result.Errors);
        }

        [Fact]
        public async Task VeryfyAndGenerateToken_ShouldReturnRefreshTokenRevoked_WhenTokenIsValidButRefreshTokenRevoked()
        {
            // Arrange
            var tokenRequest = new TokenRequestDTO
            {
                Token = GenerateValidExpiredJwtToken(),
                RefreshToken = "RevokedRefreshToken"
            };

            var revokedRefreshToken = new RefreshToken
            {
                jwtId = "validJti",
                IsUsed = false,
                IsRevoked = true,
                UserId = "testuser",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                ExpireDate = DateTime.UtcNow.AddDays(5),
                Token = "RevokedRefreshToken"
            };

            // Mock the GetRefreshTokenAsync method to return revokedRefreshToken
            var mockJwtHelper = new Mock<JwtHelper>(_tokenValidationParameters, _mockConnectionString, _mockRoleService.Object, _mockRefreshTokenService.Object, _mockUserManager.Object, _mockSignInManager.Object, _mockConfig.Object);
            mockJwtHelper.CallBase = true;
            mockJwtHelper.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(revokedRefreshToken);

            // Act
            var result = await mockJwtHelper.Object.VeryfyAndGenerateToken(tokenRequest);

            // Assert
            Assert.False(result.Result);
            Assert.Contains("This refresh token has been revoked", result.Errors);
        }

        [Fact]
        public async Task VeryfyAndGenerateToken_ShouldReturnJwtAndRefreshTokenMismatch_WhenTokenIsValidButJwtAndRefreshTokenMismatch()
        {
            // Arrange
            var tokenRequest = new TokenRequestDTO
            {
                Token = GenerateValidExpiredJwtToken(),
                RefreshToken = "MismatchedRefreshToken"
            };

            var mismatchedRefreshToken = new RefreshToken
            {
                jwtId = "mismatchedJti",
                IsUsed = false,
                IsRevoked = false,
                UserId = "testuser",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                ExpireDate = DateTime.UtcNow.AddDays(5),
                Token = "MismatchedRefreshToken"
            };

            // Mock the GetRefreshTokenAsync method to return mismatchedRefreshToken
            var mockJwtHelper = new Mock<JwtHelper>(_tokenValidationParameters, _mockConnectionString, _mockRoleService.Object, _mockRefreshTokenService.Object, _mockUserManager.Object, _mockSignInManager.Object, _mockConfig.Object);
            mockJwtHelper.CallBase = true;
            mockJwtHelper.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(mismatchedRefreshToken);

            // Act
            var result = await mockJwtHelper.Object.VeryfyAndGenerateToken(tokenRequest);

            // Assert
            Assert.False(result.Result);
            Assert.Contains("This refresh token doesn't match this JWT", result.Errors);
        }

        [Fact]
        public async Task VeryfyAndGenerateToken_ShouldReturnUserNotFound_WhenTokenIsValidButUserNotFound()
        {
            // Arrange
            var tokenRequest = new TokenRequestDTO
            {
                Token = GenerateValidExpiredJwtToken(),
                RefreshToken = "ValidRefreshToken"
            };

            var validRefreshToken = new RefreshToken
            {
                jwtId = "validJti",
                IsUsed = false,
                IsRevoked = false,
                UserId = "nonexistentUser",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                ExpireDate = DateTime.UtcNow.AddDays(5),
                Token = "ValidRefreshToken"
            };

            // Mock GetRefreshTokenAsync method to return validRefreshToken
            var mockJwtHelper = new Mock<JwtHelper>(_tokenValidationParameters, _mockConnectionString, _mockRoleService.Object, _mockRefreshTokenService.Object, _mockUserManager.Object, _mockSignInManager.Object, _mockConfig.Object);
            mockJwtHelper.CallBase = true;
            mockJwtHelper.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(validRefreshToken);

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await mockJwtHelper.Object.VeryfyAndGenerateToken(tokenRequest);

            // Assert
            Assert.False(result.Result);
            Assert.Contains("Server Error", result.Errors);
        }
    }
}