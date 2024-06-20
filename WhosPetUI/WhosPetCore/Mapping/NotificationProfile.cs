using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Outgoing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Mapping
{
    public class NotificationProfile: AutoMapper.Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationResponseDTO>()
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp));
        }

    }
}
