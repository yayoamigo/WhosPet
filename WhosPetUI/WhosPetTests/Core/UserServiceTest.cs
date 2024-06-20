using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading.Tasks;
using WhosPetCore.Services;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.Domain.Entities;

namespace WhosPetTests.Core
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetUserbyEmail_ShouldReturnNull_WhenEmailIsNullOrEmpty()
        {
            // Act
            var result = await _userService.GetUserbyEmail(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserbyEmail_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var email = "test@example.com";
            _userRepositoryMock.Setup(x => x.GetUserbyEmail(email)).ReturnsAsync((UserProfile)null);

            // Act
            var result = await _userService.GetUserbyEmail(email);

            // Assert
            Assert.Null(result);
            VerifyLogger(LogLevel.Error, "User not found");
        }

        [Fact]
        public async Task GetUserbyEmail_ShouldReturnUser_WhenUserIsFound()
        {
            // Arrange
            var email = "test@example.com";
            var user = new UserProfile { Email = email };
            _userRepositoryMock.Setup(x => x.GetUserbyEmail(email)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserbyEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        private void VerifyLogger(LogLevel logLevel, string message)
        {
            _loggerMock.Verify(
                x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
