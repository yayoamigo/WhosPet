using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.DTO.Outgoing;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.ServiceContracts.PetContracts
{
    public interface IAddPetService
    {
        Task<PetResponseDTO> AddPet(AddPetDTO addPetDTO);
    }
}
