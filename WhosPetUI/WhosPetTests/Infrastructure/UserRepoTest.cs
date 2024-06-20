using Xunit;
using Moq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.RepoContracts;
using WhosPetInfrastructure.Repos;
using Microsoft.AspNetCore.Identity;
using WhosPetAuth;
using WhosPetCore.Domain.Indentity;

namespace WhosPetTests.Infrastructure.User
{
    [Collection("Sequential-Tests")]
    public class UserRepositoryTests : IAsyncLifetime
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly UserRepository _userRepository;
        private readonly TestFixture _fixture;

        public UserRepositoryTests(TestFixture fixture)
        {
            _fixture = fixture;
            var options = new ConnectionStringOptions { ConnectionString = _fixture.ConnectionString };
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _userRepository = new UserRepository(options, _userManagerMock.Object);
        }

        public Task InitializeAsync()
        {
            return _fixture.CleanUpData();
        }

        public Task DisposeAsync()
        {
            return _fixture.CleanUpData();
        }

        private async Task SeedUserProfile()
        {
            using (var connection = new SqlConnection(_fixture.ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                    IF NOT EXISTS (SELECT 1 FROM UserProfiles WHERE Email = 'user@example.com')
                    BEGIN
                        INSERT INTO UserProfiles (Email, Name, Surname, City, Address) 
                        VALUES ('user@example.com', 'John', 'Doe', 'New York', '123 Main St');
                    END";
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        [Fact]
        public async Task GetUserbyEmail_ShouldReturnUserProfile_WhenEmailExists()
        {
            await _fixture.CleanUpData();
            await SeedUserProfile();

            // Act
            var result = await _userRepository.GetUserbyEmail("user@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal("John", result.Name);
            Assert.Equal("Doe", result.Surname);
            Assert.Equal("New York", result.City);
            Assert.Equal("123 Main St", result.Address);

            // Cleanup
            await _fixture.CleanUpData();
        }

        [Fact]
        public async Task GetUserbyEmail_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            await _fixture.CleanUpData();
            await SeedUserProfile();

            // Act
            var result = await _userRepository.GetUserbyEmail("nonexistent@example.com");

            // Assert
            Assert.Null(result);

            // Cleanup
            await _fixture.CleanUpData();
        }

        [Fact]
        public async Task GetUserbyEmail_ShouldReturnNull_WhenEmailIsNullOrEmpty()
        {
            await _fixture.CleanUpData();
            await SeedUserProfile();

            // Act
            var result = await _userRepository.GetUserbyEmail(null);

            // Assert
            Assert.Null(result);

            // Cleanup
            await _fixture.CleanUpData();
        }
    }
}
