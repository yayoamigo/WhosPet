using WhosPetCore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Domain.Entities.BaseEntities
{
    public abstract class PetBase
    {
        public string Name { get; set; }
        public AnimalTypeEnum Type { get; set; }
        public string Breed { get; set; }
        public string Color { get; set; }

        public string City { get; set; }    

        public int Age { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public string UserId { get; set; }
        public virtual UserProfile Owner { get; set; }
    }
}
