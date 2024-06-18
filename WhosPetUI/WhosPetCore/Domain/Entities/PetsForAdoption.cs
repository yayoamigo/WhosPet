using  WhosPetCore.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Domain.Entities
{
    public class PetsForAdoption : PetBase
    {
        [Key]
        public int Id { get; set; }

        public bool IsAdopted { get; set; }
    }
}
