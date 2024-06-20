using WhosPetCore.DTO.Outgoing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities;

namespace WhosPetCore.ServiceContracts.Notifications
{
    public interface INotificationService
    {
        
        Task<List<NotificationResponseDTO>> GetNotificationsByUserIdAsync(string userId);

        Task AddNotificationAsync(Notification notification);
    }
}
