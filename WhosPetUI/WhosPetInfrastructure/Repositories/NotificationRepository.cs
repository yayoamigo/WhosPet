using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WhosPetAuth;
using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.RepoContracts;

namespace WhosPetInfrastructure.Repos
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        public NotificationRepository(ConnectionStringOptions options)
        {
            _connectionString = options.ConnectionString;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "INSERT INTO Notifications (UserId, Message, Timestamp, IsRead) VALUES (@UserId, @Message, @Timestamp, @IsRead)", connection);
                command.Parameters.AddWithValue("@UserId", notification.UserProfile.Email);
                command.Parameters.AddWithValue("@Message", notification.Message);
                command.Parameters.AddWithValue("@Timestamp", notification.Timestamp);
                command.Parameters.AddWithValue("@IsRead", notification.IsRead);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(string userId)
        {
            var notifications = new List<Notification>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM Notifications WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", userId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        notifications.Add(MapReaderToNotification(reader));
                    }
                }
            }
            return notifications;
        }

       

        private Notification MapReaderToNotification(SqlDataReader reader)
        {
            return new Notification
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.GetString(reader.GetOrdinal("UserId")),
                Message = reader.GetString(reader.GetOrdinal("Message")),
                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead"))
                
            };
        }
    }
}
