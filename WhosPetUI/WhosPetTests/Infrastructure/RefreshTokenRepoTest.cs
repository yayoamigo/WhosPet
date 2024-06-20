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
    [Collection("Sequential-Tests")]
    public class RefreshTokenRepositoryTests : IAsyncLifetime
    {
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDbCommand> _mockCommand;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly TestFixture _fixture;

        public RefreshTokenRepositoryTests(TestFixture fixture)
        {
            _fixture = fixture;
            var options = new ConnectionStringOptions { ConnectionString = _fixture.ConnectionString };
            _refreshTokenRepository = new RefreshTokenRepository(options);

            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();

            _mockConnection.Setup(conn => conn.CreateCommand()).Returns(_mockCommand.Object);
            _mockConnection.Setup(conn => conn.Open()).Callback(() => { /* Simulate opening connection */ });

            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1).Verifiable();
            _mockCommand.Setup(cmd => cmd.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        }

        public Task InitializeAsync()
        {
            return _fixture.CleanUpData();
        }

        public Task DisposeAsync()
        {
            return _fixture.CleanUpData();
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
            using (var connection = new SqlConnection(_fixture.ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM RefreshTokens WHERE Token = @Token", connection))
                {
                    command.Parameters.AddWithValue("@Token", refreshToken.Token);
                    var count = (int)await command.ExecuteScalarAsync();
                    Assert.True(count > 0);
                }
            }

            // Cleanup
            await _fixture.CleanUpData();
        }
    }
}
