using Xunit;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WhosPetCore.Services;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Outgoing;
using System.Collections.Generic;

namespace WhosPetTests.Core
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly Mock<ILogger<NotificationService>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _loggerMock = new Mock<ILogger<NotificationService>>();
            _mapperMock = new Mock<IMapper>();

            _notificationService = new NotificationService(
                _notificationRepositoryMock.Object,
                _loggerMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task AddNotificationAsync_ShouldLogError_WhenNotificationIsNull()
        {
            // Act
            await _notificationService.AddNotificationAsync(null);

            // Assert
            VerifyLogger(LogLevel.Error, "Notification is null");
            _notificationRepositoryMock.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Never);
        }

        [Fact]
        public async Task AddNotificationAsync_ShouldCallRepository_WhenNotificationIsValid()
        {
            // Arrange
            var notification = new Notification { Message = "Test Notification" };

            // Act
            await _notificationService.AddNotificationAsync(notification);

            // Assert
            _notificationRepositoryMock.Verify(x => x.AddNotificationAsync(notification), Times.Once);
        }

        [Fact]
        public async Task GetNotificationsByUserIdAsync_ShouldReturnNotifications_WhenNotificationsExist()
        {
            // Arrange
            var userId = "testUser";
            var notifications = new List<Notification>
            {
                new Notification { Message = "Test Notification 1" },
                new Notification { Message = "Test Notification 2" }
            };
            var notificationResponseDTOs = new List<NotificationResponseDTO>
            {
                new NotificationResponseDTO { Message = "Test Notification 1" },
                new NotificationResponseDTO { Message = "Test Notification 2" }
            };

            _notificationRepositoryMock.Setup(x => x.GetNotificationsByUserIdAsync(userId)).ReturnsAsync(notifications);
            _mapperMock.Setup(x => x.Map<List<NotificationResponseDTO>>(notifications)).Returns(notificationResponseDTOs);

            // Act
            var result = await _notificationService.GetNotificationsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Test Notification 1", result[0].Message);
            Assert.Equal("Test Notification 2", result[1].Message);
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
