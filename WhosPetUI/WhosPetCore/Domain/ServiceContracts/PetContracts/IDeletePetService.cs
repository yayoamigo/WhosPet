using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.ServiceContracts.PetContracts
{
    public interface IDeletePetService
    {
        Task<bool> DeletePet(int id);
    }
}
