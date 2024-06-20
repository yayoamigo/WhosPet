using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.DTO.Outgoing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.ServiceContracts.PetContracts
{
    public interface IUpdatePetService
    {
        Task<PetResponseDTO> UpdatePet(UpdatePetDTO pet);
    }
}
