using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Incoming.Pets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.ServiceContracts.ReportContracts
{
    public interface IUpdatePetReport
    {
        Task<bool> FoundLostPet(int lostReportID);
    }
}
