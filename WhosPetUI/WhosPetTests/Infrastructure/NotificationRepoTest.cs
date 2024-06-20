using Xunit;
using Moq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities;
using WhosPetInfrastructure.Repos;
using WhosPetCore.DTO.Incoming.Pets;
using WhosPetAuth;
using System.Collections.Generic;

namespace WhosPetTests.Infrastructure.Notifications
{
    [Collection("Sequential-Tests")]
    public class NotificationRepositoryTests : IAsyncLifetime
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly TestFixture _fixture;

        public NotificationRepositoryTests(TestFixture fixture)
        {
            _fixture = fixture;
            var options = new ConnectionStringOptions { ConnectionString = _fixture.ConnectionString };
            _notificationRepository = new NotificationRepository(options);
        }

        public Task InitializeAsync()
        {
            return InitializeFixtureAsync();
        }

        public Task DisposeAsync()
        {
            return _fixture.CleanUpData();
        }

        private async Task InitializeFixtureAsync()
        {
            await _fixture.CleanUpData();
            await SeedUserProfile();
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
        public async Task GetNotificationsByUserIdAsync_ShouldReturnNotifications()
        {
            // Arrange
            var userId = "user@example.com";
            var notification1 = new Notification { UserProfile = new UserProfile { Email = userId }, Message = "Test message 1", Timestamp = DateTime.Now, IsRead = false };
            var notification2 = new Notification { UserProfile = new UserProfile { Email = userId }, Message = "Test message 2", Timestamp = DateTime.Now, IsRead = true };

            await _notificationRepository.AddNotificationAsync(notification1);
            await _notificationRepository.AddNotificationAsync(notification2);

            // Act
            var result = await _notificationRepository.GetNotificationsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Cleanup
            await _fixture.CleanUpData();
        }
    }
}
