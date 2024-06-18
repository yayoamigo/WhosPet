using Microsoft.AspNetCore.Identity;
using WhosPetCore.Domain.Entities;


namespace WhosPetCore.Domain.Indentity
{
    public class ApplicationUser : IdentityUser
    {
        public virtual UserProfile Profile { get; set; }
    }
}
