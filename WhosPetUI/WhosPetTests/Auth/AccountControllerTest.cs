using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WhosPetAuth.Controllers;
using WhosPetCore.ServiceContracts;
using WhosPetCore.DTO.Incoming.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using AlertaPatitasAPIUI.Helpers;
using Microsoft.AspNetCore.Http;
using WhosPetCore.Domain.Entities.Auth;
using WhosPetCore.Domain.Indentity;
using WhosPetCore.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace WhosPetAuth.Controllers.Test
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly IConfiguration _config;
        private readonly Mock<TokenValidationParameters> _tokenValidationParametersMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IUserRoleService> _roleServiceMock;
        private  AccountController _controller;

        public AccountControllerTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);

            var inMemorySettings = new Dictionary<string, string>
        {
            { "JwtConfig:Secret", "LKFJuifjdnvhLiieheyhcvyybd7ghgbdbcchv" },
            { "JwtConfig:Issuer", "test_issuer" },
            { "JwtConfig:ExpireTime", "00:30:00" },
            { "ConnectionStrings:WhosPet", "test_connection_string" }
        };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _tokenValidationParametersMock = new Mock<TokenValidationParameters>();
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            _roleServiceMock = new Mock<IUserRoleService>();

            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _config,
                _tokenValidationParametersMock.Object,
                _refreshTokenServiceMock.Object,
                _roleServiceMock.Object);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid model");

            // Act
            var result = await _controller.Register(new RegisterModel());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(badRequestResult.Value);
            Assert.False(response.Result);
            Assert.Contains("Invalid payload", response.Errors);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserAlreadyExists()
        {
            // Arrange
            var model = new RegisterModel { Email = "test@example.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(new ApplicationUser());

            // Act
            var result = await _controller.Register(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(badRequestResult.Value);
            Assert.False(response.Result);
            Assert.Contains("User already exists", response.Errors);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var model = new RegisterModel { Email = "test@example.com", Password = "Password123!" };
            var user = new ApplicationUser { Id = "1", Email = model.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddClaimsAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(IdentityResult.Success);

            _roleServiceMock.Setup(x => x.GetUserRoles(user.Id, It.IsAny<string>())).ReturnsAsync(new List<string> { "User" });
            _roleServiceMock.Setup(x => x.GetRoles(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "User" });
            _refreshTokenServiceMock.Setup(x => x.AddRefreshToken(It.IsAny<RefreshToken>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var jwtTokenGenerator = new JwtTokenGenerator(_roleServiceMock.Object, _refreshTokenServiceMock.Object);

            
            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _config,
                _tokenValidationParametersMock.Object,
                _refreshTokenServiceMock.Object,
                _roleServiceMock.Object);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.True(response.Result);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserDoesNotExist()
        {
            // Arrange
            var model = new LoginModel { Email = "test@example.com", Password = "Password123!" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(unauthorizedResult.Value);
            Assert.False(response.Result);
            Assert.Contains("User doesn't exist", response.Errors);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenLoginIsSuccessful()
        {
            // Arrange
            var model = new LoginModel { Email = "test@example.com", Password = "Password123!" };
            var user = new ApplicationUser { Id = "1", Email = model.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(model.Email, model.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        });

            _roleServiceMock.Setup(x => x.GetUserRoles(user.Id, It.IsAny<string>())).ReturnsAsync(new List<string> { "User" });
            _roleServiceMock.Setup(x => x.GetRoles(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(new List<string> { "User" });
            _refreshTokenServiceMock.Setup(x => x.AddRefreshToken(It.IsAny<RefreshToken>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var jwtTokenGenerator = new JwtTokenGenerator(_roleServiceMock.Object, _refreshTokenServiceMock.Object);

            
            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _config,
                _tokenValidationParametersMock.Object,
                _refreshTokenServiceMock.Object,
                _roleServiceMock.Object);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.True(response.Result);
        }


        [Fact]
        public async Task RefreshToken_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid model");

            // Act
            var result = await _controller.RefreshToken(new TokenRequestDTO());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(badRequestResult.Value);
            Assert.False(response.Result);
            Assert.Contains("Invalid payload", response.Errors);
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

    }
}
