using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WhosPetCore.ServiceContracts.Notifications;
using WhosPetCore.DTO.Outgoing;
using WhosPetUI.Controllers.NotificationsControllers;
using System.Collections.Generic;

namespace WhosPetTests.Controllers.NotificationsControllers
{
    public class NotificationControllerTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<NotificationContoller>> _mockLogger;
        private readonly NotificationContoller _controller;

        public NotificationControllerTests()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<NotificationContoller>>();

            _controller = new NotificationContoller(
                _mockNotificationService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetNotificationsByUser_ReturnsBadRequest_WhenEmailIsNullOrEmpty()
        {
            // Act
            var result = await _controller.getNotificationsbyuser(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetNotificationsByUser_ReturnsOkResult_WithNotifications()
        {
            // Arrange
            var email = "test@example.com";
            var notifications = new List<NotificationResponseDTO>
            {
                new NotificationResponseDTO { Message = "Notification 1" },
                new NotificationResponseDTO { Message = "Notification 2" }
            };

            _mockNotificationService.Setup(service => service.GetNotificationsByUserIdAsync(email)).ReturnsAsync(notifications);

            // Act
            var result = await _controller.getNotificationsbyuser(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(notifications, okResult.Value);
        }

        [Fact]
        public async Task GetNotificationsByUser_ReturnsOkResult_WithEmptyList_WhenNoNotifications()
        {
            // Arrange
            var email = "test@example.com";
            var notifications = new List<NotificationResponseDTO>();

            _mockNotificationService.Setup(service => service.GetNotificationsByUserIdAsync(email)).ReturnsAsync(notifications);

            // Act
            var result = await _controller.getNotificationsbyuser(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Empty((List<NotificationResponseDTO>)okResult.Value);
        }
    }
}
