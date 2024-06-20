using Xunit;
using Moq;
using System.Threading.Tasks;
using WhosPetCore.Domain.Services;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.Domain.Entities.Auth;

namespace WhosPetTests.Core
{
    public class RefreshTokenServiceTests
    {
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly RefreshTokenService _refreshTokenService;

        public RefreshTokenServiceTests()
        {
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

            _refreshTokenService = new RefreshTokenService(
                _refreshTokenRepositoryMock.Object
            );
        }

        [Fact]
        public async Task AddRefreshToken_ShouldCallRepository_WhenRefreshTokenIsValid()
        {
            // Arrange
            var refreshToken = new RefreshToken { Token = "testToken" };

            // Act
            await _refreshTokenService.AddRefreshToken(refreshToken, "dummyConnectionString");

            // Assert
            _refreshTokenRepositoryMock.Verify(x => x.AddRefreshToken(refreshToken), Times.Once);
        }
    }
}
