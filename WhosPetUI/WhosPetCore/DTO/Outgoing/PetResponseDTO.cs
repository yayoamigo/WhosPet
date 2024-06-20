using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.DTO.Outgoing
{
    public class PetResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<PetDetails>? Details { get; set; }
    }

    public class PetDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Breed { get; set; }
        public string Color { get; set; }

        public int Age { get; set; }
        public string Description { get; set; }

        public string Image { get; set; }
    }
}
