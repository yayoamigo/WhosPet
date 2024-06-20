using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Outgoing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.ServiceContracts.PetContracts
{
    public interface IGetPetService
    {
        Task<PetResponseDTO> GetPetsByShelter(string name);

        Task<PetResponseDTO> GetPetsByCity(string city);

        Task<PetResponseDTO> GetUserPets();
        Task<PetResponseDTO> GetPetByType(string type);

    }
}
