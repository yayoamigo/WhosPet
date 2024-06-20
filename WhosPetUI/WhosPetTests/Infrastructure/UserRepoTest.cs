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
    public class UserRepositoryTests : IAsyncLifetime
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly UserRepository _userRepository;
        private readonly string _connectionString = "Data Source=DESKTOP-ER2DF6Q;Initial Catalog=WhosPetTest;User ID=sa;Password=12345;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True";

        public UserRepositoryTests()
        {
            var options = new ConnectionStringOptions { ConnectionString = _connectionString };
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _userRepository = new UserRepository(options, _userManagerMock.Object);
        }

        public async Task InitializeAsync()
        {
            await CleanUpData();
            await SeedUserProfile();
        }

        public async Task DisposeAsync()
        {
            await CleanUpData();
        }

        private async Task CleanUpData()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        DELETE FROM UserProfiles;";
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task SeedUserProfile()
        {
            using (var connection = new SqlConnection(_connectionString))
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
            // Act
            var result = await _userRepository.GetUserbyEmail("user@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal("John", result.Name);
            Assert.Equal("Doe", result.Surname);
            Assert.Equal("New York", result.City);
            Assert.Equal("123 Main St", result.Address);
        }

        [Fact]
        public async Task GetUserbyEmail_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Act
            var result = await _userRepository.GetUserbyEmail("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserbyEmail_ShouldReturnNull_WhenEmailIsNullOrEmpty()
        {
            // Act
            var result = await _userRepository.GetUserbyEmail(null);

            // Assert
            Assert.Null(result);
        }
    }
}
