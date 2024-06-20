using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Incoming.Users;
using WhosPetCore.DTO.Outgoing;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Mapping
{
    public class UserMapProfile: Profile
    {
        public UserMapProfile()
        {
            CreateMap<UpdateProfileDTO, UserProfile>();
            CreateMap<UserProfile, ProfileResponseDTO>();

        }
    }
}
