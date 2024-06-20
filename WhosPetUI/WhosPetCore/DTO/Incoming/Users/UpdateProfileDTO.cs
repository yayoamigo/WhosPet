using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.DTO.Incoming.Users
{
    public class UpdateProfileDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string SurName { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Adress { get; set; }
    }
}
