using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.DTO.Incoming.Pets
{
    public class UpdatePetDTO
    {
        [Required]
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
