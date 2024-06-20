using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhosPetCore.Domain.Services;
using WhosPetCore.Domain.RepoContracts;

namespace WhosPetTests.Core
{
    public class UserRoleServiceTests
    {
        private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
        private readonly UserRoleService _userRoleService;

        public UserRoleServiceTests()
        {
            _userRoleRepositoryMock = new Mock<IUserRoleRepository>();

            _userRoleService = new UserRoleService(
                _userRoleRepositoryMock.Object
            );
        }

        [Fact]
        public async Task GetUserRoles_ShouldReturnRoles_WhenUserIdIsValid()
        {
            // Arrange
            var userId = "testUser";
            var expectedRoles = new List<string> { "Admin", "User" };
            _userRoleRepositoryMock.Setup(x => x.GetUserRoles(userId)).ReturnsAsync(expectedRoles);

            // Act
            var result = await _userRoleService.GetUserRoles(userId, "dummyConnectionString");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedRoles, result);
            _userRoleRepositoryMock.Verify(x => x.GetUserRoles(userId), Times.Once);
        }

        [Fact]
        public async Task GetRoles_ShouldReturnRoles_WhenUserRolesAreValid()
        {
            // Arrange
            var userRoles = new List<string> { "Admin", "User" };
            var expectedRoles = new List<string> { "Admin", "User", "Guest" };
            _userRoleRepositoryMock.Setup(x => x.GetRoles(userRoles)).ReturnsAsync(expectedRoles);

            // Act
            var result = await _userRoleService.GetRoles(userRoles, "dummyConnectionString");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedRoles, result);
            _userRoleRepositoryMock.Verify(x => x.GetRoles(userRoles), Times.Once);
        }
    }
}
