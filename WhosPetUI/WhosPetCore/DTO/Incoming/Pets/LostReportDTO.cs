using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.DTO.Incoming.Pets
{
    public class LostReportDTO
    {
        public string PetName { get; set; }
        public string Description { get; set; }
        public DateTime DateLost { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }
    }
}
