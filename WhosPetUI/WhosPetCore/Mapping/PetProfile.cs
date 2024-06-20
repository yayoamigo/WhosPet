using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.DTO.Outgoing;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Mapping
{
    public class PetProfile: AutoMapper.Profile
    {
        public PetProfile()
        {
            CreateMap<AddPetDTO, Pets>();
            CreateMap<Pets, PetDetails>();
            CreateMap<UpdatePetDTO, Pets>();

        }

    }
}
