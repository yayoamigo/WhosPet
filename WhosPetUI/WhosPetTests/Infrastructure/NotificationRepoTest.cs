using Xunit;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities;
using WhosPetInfrastructure.Repos;
using WhosPetAuth;
using System.Collections.Generic;
using System;

namespace WhosPetTests.Infrastructure.Notifications
{
    public class NotificationRepositoryTests : IAsyncLifetime
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly string _connectionString = "Data Source=DESKTOP-ER2DF6Q;Initial Catalog=WhosPetTest;User ID=sa;Password=12345;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True";

        public NotificationRepositoryTests()
        {
            var options = new ConnectionStringOptions { ConnectionString = _connectionString };
            _notificationRepository = new NotificationRepository(options);
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
                        DELETE FROM Notifications;
                        DELETE FROM Pets;
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
        }
    }
}
