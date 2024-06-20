using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Domain.Entities
{
    public class LostPetReport
    {
        public  int  Id { get; set; }

        public string UserId { get; set; }
        public UserProfile UserProfile { get; set; }

        public Pets Pet { get; set; }
        public int PetId { get; set; }

        public string PetName { get; set; }
        public string Description { get; set; }

        public DateTime Date { get; set; }

        public string City { get; set; }

        public double longitude { get; set; }

        public double latitude { get; set; }

        public string Image { get; set; }

        public bool IsFound { get; set; }

        public bool IsActive { get; set; }
    }
}
