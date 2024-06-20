using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.DTO.Outgoing
{
    public class ProfileResponseDTO
    {
        public string name { get; set; }
        public string surname { get; set; }

        public string city { get; set; }

        public string adress { get; set; }
    }
}
