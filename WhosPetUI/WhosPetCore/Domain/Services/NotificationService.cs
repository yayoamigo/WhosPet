using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.DTO.Outgoing;
using WhosPetCore.ServiceContracts.Notifications;

namespace WhosPetCore.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            if (notification == null)
            {
                _logger.LogError("Notification is null");
                return;
            }

            await _notificationRepository.AddNotificationAsync(notification);


        }

        public async Task<List<NotificationResponseDTO>> GetNotificationsByUserIdAsync(string userId)
        {
            var reponse = await _notificationRepository.GetNotificationsByUserIdAsync(userId);

            var responseDto = _mapper.Map<List<NotificationResponseDTO>>(reponse);

            return responseDto;
        }

       
    }
}
