using WhosPetCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.DTO.Outgoing
{
    public class LostPetResponseDTO
    {
        public int Id { get; set; }

        public string PetName { get; set; }
        public string Description { get; set; }

        public DateTime Date { get; set; }

        public string Location { get; set; }

        public double longitude { get; set; }

        public double latitude { get; set; }

        public string Image { get; set; }

        public bool IsFound { get; set; }

        public bool IsActive { get; set; }
    }
}
