using WhosPetCore.Domain.Indentity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosPetCore.Domain.Enums;
using WhosPetCore.Domain.Entities;

namespace WhosPetCore.Domain.Entities
{
    public class UserProfile
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string City { get; set; }
        public string Address { get; set; }

        public virtual ICollection<Pets> Pets { get; set; }
        public virtual ICollection<LostPetReport> LostPetReports { get; set; }
        public virtual ICollection<PetsForAdoption> PetsForAdoption { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
