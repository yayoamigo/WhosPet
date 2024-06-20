using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Outgoing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.ServiceContracts.ReportContracts
{
    public interface IGetLostReports
    {

        Task<IEnumerable<LostPetResponseDTO>> GetLostPetReportByCity(string City);

    }
}

