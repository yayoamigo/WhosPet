using WhosPetCore.ServiceContracts.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace WhosPetUI.Controllers.NotificationsControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationContoller : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationContoller> _logger;

        public NotificationContoller(INotificationService notificationService, ILogger<NotificationContoller> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> getNotificationsbyuser(string email)
        {
            if(string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required");
            }
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(email);
            return Ok(notifications);
        }
    }
}
