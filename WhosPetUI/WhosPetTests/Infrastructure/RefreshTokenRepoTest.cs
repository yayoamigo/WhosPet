using Xunit;
using Moq;
using System.Data;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities.Auth;
using WhosPetInfrastructure.Repositories;
using WhosPetAuth;
using System;
using System.Data.SqlClient;

namespace WhosPetTests.Infrastructure.RefreshTokenRepoTests
{
    public class RefreshTokenRepositoryTests : IAsyncLifetime
    {
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDbCommand> _mockCommand;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly string _connectionString = "Data Source=DESKTOP-ER2DF6Q;Initial Catalog=WhosPetTest;User ID=sa;Password=12345;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True";

        public RefreshTokenRepositoryTests()
        {
            var options = new ConnectionStringOptions { ConnectionString = _connectionString };
            _refreshTokenRepository = new RefreshTokenRepository(options);

            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();

            _mockConnection.Setup(conn => conn.CreateCommand()).Returns(_mockCommand.Object);
            _mockConnection.Setup(conn => conn.Open()).Callback(() => { /* Simulate opening connection */ });

            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1).Verifiable();
            _mockCommand.Setup(cmd => cmd.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        }

        public async Task InitializeAsync()
        {
            await CleanUpData();
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
                    command.CommandText = "DELETE FROM RefreshTokens";
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        [Fact]
        public async Task AddRefreshToken_ShouldAddTokenToDatabase()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                UserId = "user123",
                Token = "token123",
                jwtId = "12345678-1234-1234-1234-123456789012",
                IsUsed = false,
                IsRevoked = false,
                ExpireDate = DateTime.UtcNow.AddMinutes(5),
                CreatedDate = DateTime.UtcNow
            };

            // Act
            await _refreshTokenRepository.AddRefreshToken(refreshToken);

            // Assert

            // Verify token in the database
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM RefreshTokens WHERE Token = @Token", connection))
                {
                    command.Parameters.AddWithValue("@Token", refreshToken.Token);
                    var count = (int)await command.ExecuteScalarAsync();
                    Assert.Equal(1, count); // There should be one token with the specified value
                }
            }

        }
    }
}
