using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.DTO.Incoming.Pets
{
    public class LostReportUpdateDTO
    {
        public int Id { get; set; }       
        public bool IsFound { get; set; }
        public bool IsActive { get; set; }

        public string City { get; set; }

        public double longitude { get; set; }

        public double latitude { get; set; }

        public string Image { get; set; }

    }
}
