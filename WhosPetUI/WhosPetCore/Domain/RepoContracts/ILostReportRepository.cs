using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Incoming.Pets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Domain.RepoContracts
{
    public interface ILostReportRepository
    {
        Task<int> AddLostPetReportAsync(LostPetReport lostPetReport);

        Task<IEnumerable<LostPetReport>> GetLostPetReportByCityAsync(string City);

        Task<bool> FoundLostPet(int lostReportID);
    }
}
